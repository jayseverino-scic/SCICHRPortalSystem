using System.ComponentModel.DataAnnotations;

namespace SCICHRPortal.Web.Models.ViewModels.Administration
{
    public class TimekeepingAdminSetupViewModel
    {
        public int SetupId { get; set; }
        [Required(ErrorMessage ="Shift Late Grace Period is required.")]
        public int ShiftLateMinuteGracePeriod { get; set; }
        [Required(ErrorMessage ="Break Late Grace Period is required.")]
        public int BreakLateMinuteGracePeriod { get; set; }
        [Required(ErrorMessage ="Shift Total Late Limit is required.")]
        public int ShiftLateTotalMinuteLimit { get; set; }
        [Required(ErrorMessage ="Break Total Late Limite is required.")]
        public int BreakLateTotalMinuteLimit { get; set; }
        [Required(ErrorMessage ="No Timelog Limit Count is required.")]
        public int NoTimeLogCountLimit { get; set; }
        [Required(ErrorMessage ="No Leave Limit Count is required.")]
        public int NoLeaveAbsentCountLimit { get; set; }
        [Required(ErrorMessage = "Rest Day is required.")]
        public string? RestDays { get; set;  }
    }
}
