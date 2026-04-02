using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeShiftUpdateRequestModel
    {
        public int AssignedShiftId { get; set; }
        [Required(ErrorMessage = "Shift is required.")]
        public int ShiftId { get; set; }
        [Required(ErrorMessage = "Employee is required.")]
        public int EmployeeId { get; set; }
        [Required(ErrorMessage = "Department is required.")]
        public int DepartmentId { get; set; }
        
        public DateTime? ShiftDate { get; set; }
        
        public DateTime? ShiftStart { get; set; }
        
        public DateTime? ShiftEnd { get; set; }
        public DateTime? BreakStart { get; set; }
        public DateTime? BreakEnd { get; set; }
        public bool IsFlexibleShift { get; set; }
        public bool IsFlexibleBreak { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
        public Boolean? IsAssigned { get; set; }
        public Employee? Employee { get; set; }
        public Department? Department { get; set; }
        public Shift? Shift { get; set; }
    }
}
