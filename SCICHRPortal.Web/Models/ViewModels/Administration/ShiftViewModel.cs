using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class ShiftViewModel
    {
        [MaxLength(100)]
        [Required(ErrorMessage = "Shift Name required.")]
        public string? ShiftName { get; set; }
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
        public int ShiftLateMinuteGracePeriod { get; set; }
        public int ShiftLateTotalMinuteLimit { get; set; }
        public int NoTimeLogCountLimit { get; set; }
        public int NoLeaveAbsentCountLimit { get; set; }
        public string? RestDays {  get; set; }
    }
}
