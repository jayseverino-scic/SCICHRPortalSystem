
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;

namespace SCICHRPortal.API.Models.RequestModels.Authenticated.Administration
{
    public class EmployeeAttendanceSummary
    {
        public string? EmployeeNo { get; set; }
        public string? EmployeeName { get; set; }
        public double ShiftTotalHours { get; set; }
        public double RegularTotalHours { get; set; }
        public double TotalLoggedHours { get; set; }
        public double ShiftLateTotalMinutes { get; set; }
        public double ShiftUndertimeTotalMinutes { get; set; }
        public double BreakUndertimeTotalMinutes { get; set; }
        public double BreakLateTotalMinutes { get; set; }
        public double OvertimeTotalHours { get; set; }
        public double NightDifferentialTotalHours { get; set; }
        public double HolidayTotalHours { get; set; }
        public double HolidayOvertimeTotalHours { get; set; }
        public double HolidayNightDifferentialTotalHours { get; set; }
        public double SpecialHolidayTotalHours { get; set; }
        public double SpecialHolidayOvertimeTotalHours { get; set; }
        public double SpecialHolidayNightDifferentialTotalHours { get; set; }
        public double RestDayTotalHours { get; set; }
        public double RestDayOvertimeTotalHours { get; set; }
        public double RestDayNightDifferentialTotalHours { get; set; }
    }
}
