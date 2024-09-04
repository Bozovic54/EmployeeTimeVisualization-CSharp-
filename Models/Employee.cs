using Microsoft.VisualBasic;
using NuGet.Packaging.Signing;

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
        public int TotalHoursWorked { get; set; }
        public bool IsBelowWorkingHoursLimit { get; set; }

    }
}
