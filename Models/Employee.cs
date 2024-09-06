using Microsoft.VisualBasic;
using NuGet.Packaging.Signing;
using System.ComponentModel.DataAnnotations;

namespace C_Project.Models
{
    public class Employee
    {
        public string Id { get; set; }
        public string EmployeeName { get; set; }
        public DateTime StarTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public string EntryNotes { get; set; }
        public DateTime? DeletedOn { get; set; }

        [DisplayFormat(DataFormatString = "{0:F0}")]
        public decimal TotalHoursWorked { get; set; }
        public bool IsBelowWorkingHoursLimit { get; set; }

    }
}
