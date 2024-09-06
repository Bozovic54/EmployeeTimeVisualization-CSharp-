using C_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;
using System.Globalization;
using System.Numerics;

public class EmployeesController : Controller
{
    private readonly HttpClient _httpClient;
    public EmployeesController(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<IActionResult> Index()
    {
        try
        {
            var response = await _httpClient.GetAsync("https://rc-vault-fap-live-1.azurewebsites.net/api/gettimeentries?code=vO17RnE8vuzXzPJo5eaLLjXjmRW07law99QTD90zat9FfOQJKKUcgQ==");
            if (response.IsSuccessStatusCode)

            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<Employee>>(jsonData);
                List<Employee> employees = new List<Employee>();

                foreach (var employee in data)
                {
                    if (string.IsNullOrWhiteSpace(employee.EmployeeName))
                    {
                        continue;
                    }
                    if (!employees.Any(e => e.EmployeeName == employee.EmployeeName))
                    {
                        Employee newEmployee = new Employee
                        {
                            Id = Guid.NewGuid().ToString(),
                            EmployeeName = employee.EmployeeName,
                            EntryNotes = employee.EntryNotes,
                            StarTimeUtc = employee.StarTimeUtc,
                            EndTimeUtc = employee.EndTimeUtc,
                            DeletedOn = employee.DeletedOn,
                            TotalHoursWorked = 0,
                        };
                        newEmployee.TotalHoursWorked = CalculateTotalWorkedHours(newEmployee);
                        employees.Add(newEmployee);
                    }
                    else
                    {
                        var existingEmployee = employees.Find(e => e.EmployeeName == employee.EmployeeName);

                        existingEmployee.StarTimeUtc = employee.StarTimeUtc;
                        existingEmployee.EndTimeUtc = employee.EndTimeUtc;
                        if (employee.StarTimeUtc < employee.EndTimeUtc)
                        {

                            decimal totalTime = CalculateTotalWorkedHours(existingEmployee);
                            if (totalTime > 0)
                            {
                                existingEmployee.TotalHoursWorked += totalTime;
                            }
                            if (employee.EmployeeName.Equals("Abhay Singh"))
                            {
                                Console.WriteLine(existingEmployee.TotalHoursWorked.ToString());
                            }
                        }
                        

                    }
                }
                foreach (var employee in employees)
                {
                    employee.IsBelowWorkingHoursLimit = employee.TotalHoursWorked < 100;
                    employee.TotalHoursWorked = (int)Math.Round(employee.TotalHoursWorked);
                }

                var sortedEmployees = employees.OrderByDescending(e => e.TotalHoursWorked).ToList();
                TempData["Employees"] = JsonConvert.SerializeObject(sortedEmployees);
                
                return View(sortedEmployees);
            }
            else
            {
                return View("Error", new ErrorViewModel { RequestId = "API call failed" });
            }
        }
        catch (Exception ex)
        {
            return View("Error", new ErrorViewModel { RequestId = $"Exception: {ex.Message}" });
        }
    }
    private decimal CalculateTotalWorkedHours(Employee employee)
    {
        decimal preciseHours = 0;
        DateTime startTime = employee.StarTimeUtc;
        DateTime endTime = employee.EndTimeUtc;
        //TimeSpan duration = endTime - startTime;

        TimeSpan duration = endTime.Subtract(startTime);
        int totalMinutes = (int)duration.TotalMinutes;

        int totalHours = totalMinutes / 60;
        int remainingMinutes = totalMinutes % 60;
        preciseHours = totalHours + (decimal)remainingMinutes / 60;
        if (preciseHours < 0)
        {
            preciseHours = 0;
        }
        //double totalHours = (endTime - startTime).TotalHours;
        //long millisecondsWorked = (long)duration.TotalMilliseconds;

        //// Konvertujte milisekunde u sate
        //totalHours = (int)(millisecondsWorked / (1000 * 60 * 60));

        //totalHours = (int)duration.TotalHours;
        return preciseHours; // Vratite totalHours ili neki drugi odgovarajući rezultat.
    }


    [HttpPost]
    public IActionResult GeneratePieChart()
    {
        try
        {
            var employees = JsonConvert.DeserializeObject<List<Employee>>(TempData["Employees"].ToString());
            decimal totalHoursWorked = employees.Sum(e => e.TotalHoursWorked);

            var plotModel = new PlotModel { Title = "Pie Chart" };
            var pieSeries = new PieSeries
            {
                InsideLabelFormat = "{0:0.##}%",
                OutsideLabelFormat = "{1:0.##}",
                FontWeight = FontWeights.Bold,
                StrokeThickness = 1,
                FontSize = 14
            };

            foreach (var employee in employees)
            {
                float percentage = (float)employee.TotalHoursWorked / (int)totalHoursWorked * 100;
                pieSeries.Slices.Add(new PieSlice(employee.EmployeeName, percentage));
            }

            plotModel.Series.Add(pieSeries);

            using (var stream = new MemoryStream())
            {
                var exporter = new PngExporter { Width = 1000, Height = 800 };
                exporter.Export(plotModel, stream);
                string fileName = $"piechart_{DateTime.Now.Ticks}.png";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.CopyTo(fileStream);
                }

                return RedirectToAction("Index", new { imagePath = $"/images/{fileName}" });
            }
        }
        catch (Exception ex)
        {
            return View("Error", new ErrorViewModel { RequestId = $"Exception: {ex.Message}" });
        }
    }
}

