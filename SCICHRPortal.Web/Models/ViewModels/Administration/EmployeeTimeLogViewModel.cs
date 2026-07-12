using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class EmployeeTimeLogViewModel
    {
        public int TimeLogId { get; set; }
        [Required(ErrorMessage = "Employee is required.")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage ="Employee number is required.")]
        public string? EmployeeNo { get; set; }
        [Required(ErrorMessage ="Employee name is required.")]
        public string? EmployeeName { get; set; }
        [Required(ErrorMessage ="Date log is required.")]
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public DateTime? TimeIn { get; set; }
        public DateTime? TimeOut { get; set; }
        public DateTime? ShiftStart { get; set; }
        public DateTime? ShiftEnd { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
        public string? ProjectTimeIn { get; set; }
        public string? ProjectTimeOut { get; set; }
        public string? DeviceTimeIn { get; set; }
        public string? DeviceTimeOut { get; set; }
    }
}
