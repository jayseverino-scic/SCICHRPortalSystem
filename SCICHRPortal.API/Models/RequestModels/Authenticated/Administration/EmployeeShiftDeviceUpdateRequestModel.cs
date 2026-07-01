using Microsoft.VisualBasic;
using System.ComponentModel.DataAnnotations;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.XscribeTables;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeShiftDeviceUpdateRequestModel
    {
        public int AssignedShiftId { get; set; }
        [Required(ErrorMessage = "Shift is required.")]
        public int ShiftId { get; set; }
        [Required(ErrorMessage = "Employee is required.")]
        public int EmployeeId { get; set; }
        public DateTime? ShiftDate { get; set; }
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
        public bool IsFlexibleShift { get; set; }
        public bool IsNoShift { get; set; }
        public bool IsNoBreak { get; set; }
        public string? DeviceName { get; set; }
        public Boolean? IsAssigned { get; set; }
        public XEmployee? Employee { get; set; }
        public Shift? Shift { get; set; }
    }
}
