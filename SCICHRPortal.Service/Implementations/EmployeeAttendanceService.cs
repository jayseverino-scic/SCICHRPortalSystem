using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.Service.Implementations
{
    public class EmployeeAttendanceService : IEmployeeAttendanceService
    {
        public IEmployeeAttendanceRepository EmployeeAttendanceRepository { get; }

        private IEmployeeShiftRepository EmployeeShiftRepository { get; }
        private IShiftRepository ShiftRepository { get; }
        private ICutOffRepository CutOffRepository { get; }
        private IHolidayRepository HolidayRepository { get; }
        private IProjectService ProjectService { get; }

        public EmployeeAttendanceService(
            IEmployeeAttendanceRepository employeeAttendanceRepository,
            IEmployeeShiftRepository employeeShiftRepository,
            IShiftRepository shiftRepository,
            ICutOffRepository cutOffRepository,
            IHolidayRepository holidayRepository,
            IProjectService projectService)
        {
            EmployeeAttendanceRepository = employeeAttendanceRepository;
            EmployeeShiftRepository = employeeShiftRepository;
            ShiftRepository = shiftRepository;
            CutOffRepository = cutOffRepository;
            HolidayRepository = holidayRepository;
            ProjectService = projectService;
        }

        public async Task<bool> UpdateAsync(EmployeeAttendance entity)
        {
            return await EmployeeAttendanceRepository.UpdateAsync(entity);
        }

        public async Task<Tuple<IEnumerable<EmployeeAttendance>, int>> FilterAsync(int pageNumber, int pageSize, string searchKeyword)
        {
            return await EmployeeAttendanceRepository.FilterAsync(pageNumber, pageSize, searchKeyword);
        }

        public async Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceFilter(int projectId, DateTime attendanceDate)
        {
            return await EmployeeAttendanceRepository.EmployeeAttendanceFilter(projectId, attendanceDate);
        }
        public async Task<IEnumerable<EmployeeAttendance>> EmployeeAttendanceCutOffFilter(int projectId, DateTime fromDate, DateTime toDate)
        {
            return await EmployeeAttendanceRepository.EmployeeAttendanceCutOffFilter(projectId, fromDate, toDate);
        }

        public async Task<IEnumerable<EmployeeAttendanceSummary>?> GetCutOffSummaryAsync(int projectId, int cutOffId)
        {
            var cutOff = await CutOffRepository.GetAsync(cutOffId);

            if (cutOff == null)
                return null;

            var attendances = await EmployeeAttendanceRepository.EmployeeAttendanceCutOffFilter(projectId, cutOff.StartDate, cutOff.EndDate);
            var summaries = new List<EmployeeAttendanceSummary>();

            foreach (var employeeAttendances in attendances.GroupBy(attendance => attendance.EmployeeId))
            {
                var employee = employeeAttendances.First().Employee;
                if (employee == null || employee.Deleted)
                    continue;

                var periods = await EmployeeShiftRepository.GetPeriodsAsync([employee.EmployeeId], cutOff.StartDate.Date, cutOff.EndDate.Date.AddDays(2));
                summaries.Add(await BuildEmployeeSummaryAsync(employee, employeeAttendances, periods));
            }

            return summaries;
        }

        private async Task<EmployeeAttendanceSummary> BuildEmployeeSummaryAsync(
            Employee employee,
            IEnumerable<EmployeeAttendance> attendances,
            IEnumerable<EmployeeShift> periods)
        {
            var summary = new EmployeeAttendanceSummary();
            var definitions = new Dictionary<int, Shift>();
            var holidays = await HolidayRepository.GetAllAsync();
            var days = Enum.GetValues<DaysEnum>();

            foreach (var attendance in attendances)
            {
                var shift = await GetHistoricalShiftAsync(attendance.TimeIn, periods, definitions);
                var isRestDay = IsRestDay(attendance.TimeIn, shift, days);
                var isHoliday = holidays.Any(holiday => holiday.HolidayDate!.Value.Day == attendance.TimeIn.Day);

                AddAttendanceTotals(summary, attendance, isHoliday, isRestDay);
            }

            summary.EmployeeNo = employee.EmployeeNo;
            summary.EmployeeName = employee.LastName + ", " + employee.FirstName;
            return summary;
        }

        private async Task<Shift?> GetHistoricalShiftAsync(
            DateTime timeIn,
            IEnumerable<EmployeeShift> periods,
            Dictionary<int, Shift> definitions)
        {
            var historical = EmployeeShiftSessions.At(periods, DateTime.SpecifyKind(timeIn, DateTimeKind.Unspecified));
            if (historical == null)
                return null;

            if (definitions.TryGetValue(historical.ShiftId, out var shift))
                return shift;

            shift = await ShiftRepository.GetAsync(historical.ShiftId);
            if (shift != null)
                definitions[historical.ShiftId] = shift;

            return shift;
        }

        private static bool IsRestDay(DateTime timeIn, Shift? shift, DaysEnum[] days)
        {
            var dayNames = days.Select(day => day.ToString()).ToArray();
            var dayIndex = Array.IndexOf(dayNames, timeIn.DayOfWeek.ToString()) + 1;
            var restDays = (shift?.RestDays ?? "").Split(';', StringSplitOptions.TrimEntries);

            return Array.IndexOf(restDays, dayIndex.ToString()) < 0;
        }

        private static void AddAttendanceTotals(
            EmployeeAttendanceSummary summary,
            EmployeeAttendance attendance,
            bool isHoliday,
            bool isRestDay)
        {
            summary.ShiftTotalHours += attendance.ShiftHours;
            summary.RegularTotalHours += attendance.RegularHour;
            summary.TotalLoggedHours += attendance.TotalLoggedHours;
            summary.ShiftLateTotalMinutes += attendance.ShiftLate;
            summary.ShiftUndertimeTotalMinutes += attendance.ShiftUndertime;
            summary.OvertimeTotalHours += attendance.ApprovedOT ? attendance.OTHours : 0;
            summary.NightDifferentialTotalHours += attendance.NDHours;
            summary.HolidayTotalHours += attendance.ApprovedHoliday && isHoliday ? attendance.RegularHour : 0;
            summary.HolidayOvertimeTotalHours += attendance.ApprovedHolidayOT && isHoliday ? attendance.OTHours : 0;
            summary.HolidayNightDifferentialTotalHours += attendance.ApprovedHoliday && isHoliday ? attendance.NDHours : 0;
            summary.SpecialHolidayTotalHours += attendance.ApprovedSPHoliday && isHoliday ? attendance.RegularHour : 0;
            summary.SpecialHolidayOvertimeTotalHours += attendance.ApprovedSPHolidayOT && isHoliday ? attendance.OTHours : 0;
            summary.SpecialHolidayNightDifferentialTotalHours += attendance.ApprovedSPHoliday && isHoliday ? attendance.NDHours : 0;
            summary.RestDayTotalHours += attendance.ApprovedRestDay && isRestDay ? attendance.RegularHour : 0;
            summary.RestDayOvertimeTotalHours += attendance.ApprovedRestDayOT && isRestDay ? attendance.OTHours : 0;
            summary.RestDayNightDifferentialTotalHours += attendance.ApprovedRestDay ? attendance.NDHours : 0;
        }

        public async Task<EmployeeAttendance> GetAsync(int id)
        {
            return await EmployeeAttendanceRepository.GetAsync(id);
        }
        public async Task<IEnumerable<EmployeeAttendance>> GetAllAsync()
        {
            return await EmployeeAttendanceRepository.GetAllAsync();
        }

        public async Task InsertAsync(EmployeeAttendance entity)
        {
            await EmployeeAttendanceRepository.InsertAsync(entity);
        }
        public async Task RemoveRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            await EmployeeAttendanceRepository.RemoveRangeAsync(employeeAttendances);
        }
        public async Task UpdateRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            await EmployeeAttendanceRepository.UpdateRangeAsync(employeeAttendances);
        }
        public async Task InsertRangeAsync(List<EmployeeAttendance> employeeAttendances)
        {
            await EmployeeAttendanceRepository.InsertRangeAsync(employeeAttendances);
        }
    }
}
