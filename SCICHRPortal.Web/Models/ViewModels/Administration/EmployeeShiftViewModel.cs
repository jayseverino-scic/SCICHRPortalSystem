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
        [Required(ErrorMessage ="Project is required.")]
        public int ProjectId { get; set; }
        [Required(ErrorMessage ="Shift date is required.")]
        public DateTime ShiftDate { get; set; }
        public DateTime? MondayShiftStart { get; set; }
        public DateTime? MondayShiftEnd { get; set; }
        public DateTime? TuesdayShiftStart { get; set; }
        public DateTime? TuesdayShiftEnd { get; set; }
        public DateTime? WednesdayShiftStart { get; set; }
        public DateTime? WednesdayShiftEnd { get; set; }
        public DateTime? ThursdayShiftStart { get; set; }
        public DateTime? ThursdayShiftEnd { get; set; }
        public DateTime? FridayShiftStart { get; set; }
        public DateTime? FridayShiftEnd { get; set; }
        public DateTime? SaturdayShiftStart { get; set; }
        public DateTime? SaturdayShiftEnd { get; set; }
        public DateTime? SundayShiftStart { get; set; }
        public DateTime? SundayShiftEnd { get; set; }
        public Boolean IsAssigned { get; set; }
    }
}
