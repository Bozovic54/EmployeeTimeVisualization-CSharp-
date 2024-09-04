using C_Project.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;


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
                    else if (!employees.Any(e => e.EmployeeName == employee.EmployeeName))
                    {
                        Employee newEmployee = new Employee
                        {
                            Id = Guid.NewGuid().ToString(),
                            EmployeeName = employee.EmployeeName,
                            EntryNotes = employee.EntryNotes,
                            StarTimeUtc = employee.StarTimeUtc,
                            EndTimeUtc = employee.EndTimeUtc,
                            DeletedOn = employee.DeletedOn,
                            TotalHoursWorked = employee.TotalHoursWorked,

                        };
                        employees.Add(newEmployee);
                    }
                    else
                    {
                        var existingEmployee = employees.Find(e => e.EmployeeName == employee.EmployeeName);
                        TimeSpan duration = existingEmployee.EndTimeUtc.Subtract(existingEmployee.StarTimeUtc);

                        int totalHours = (int)duration.TotalHours;
                        if (totalHours > 0)
                        {
                            existingEmployee.TotalHoursWorked += totalHours;
                        }
                    }
                }
                foreach (var employee in employees)
                {
                    employee.IsBelowWorkingHoursLimit = employee.TotalHoursWorked < 100;
                }

                var sortedEmployees = employees.OrderByDescending(e => e.TotalHoursWorked).ToList();
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
}
