using SCICHRPortal.Data.Entities;
using System.ComponentModel.DataAnnotations;
using System.Security.Permissions;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class EmployeeShiftViewModel
    {
        public int AssignedShiftId { get; set; }
        [Required(ErrorMessage = "Shift is required.")]
        public int ShiftId { get; set; }
        [Required(ErrorMessage ="Employee is required.")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage ="Department is required.")]
        public int DepartmentId { get; set; }
        [Required(ErrorMessage ="Shift date is required.")]
        public DateTime ShiftDate { get; set; }
        [Required(ErrorMessage ="Shift start is required.")]
        public DateTime ShiftStart { get; set; }
        [Required(ErrorMessage ="Shift end is required.")]
        public DateTime ShiftEnd { get; set; }
        [Required(ErrorMessage ="Break Start is required.")]
        public DateTime BreakStart { get; set; }
        [Required(ErrorMessage ="Break End is required.")]
        public DateTime BreakEnd { get; set; }
        public Boolean IsAssigned { get; set; }
    }
}
