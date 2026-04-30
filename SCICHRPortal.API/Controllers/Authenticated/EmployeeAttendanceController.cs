using iText.Kernel.Geom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Data.Enums;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class EmployeeAttendanceController : ControllerBase
    {
        private IEmployeeAttendanceService EmployeeAttendanceService { get; }
        private IEmployeeService EmployeeService { get; }
        private IShiftService ShiftService { get; }
        private IDepartmentService DepartmentService { get; }
        private IEmployeeTimeLogService EmployeeTimeLogService { get; }
        private ITimekeepingAdminSetupService TimekeepingAdminSetupService { get; }
        private ICutOffService CutOffService { get; }
        private IHolidayService HolidayService { get; }
        public EmployeeAttendanceController(IEmployeeAttendanceService employeeAttendanceService, IEmployeeService employeeService, IShiftService shiftService, IDepartmentService departmentService, IEmployeeTimeLogService employeeTimeLogService, ITimekeepingAdminSetupService timekeepingAdminSetupService, ICutOffService cutOffService, IHolidayService holidayService)
        {
            EmployeeAttendanceService = employeeAttendanceService;
            EmployeeService = employeeService;
            ShiftService = shiftService;
            DepartmentService = departmentService;
            EmployeeTimeLogService = employeeTimeLogService;
            TimekeepingAdminSetupService = timekeepingAdminSetupService;
            CutOffService = cutOffService;
            HolidayService = holidayService;
        }

        [Authorize] 
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var employeeAttendances = await EmployeeAttendanceService.GetAllAsync();
            return Ok(employeeAttendances);
        }

        [Authorize]
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await EmployeeAttendanceService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            IEnumerable<EmployeeAttendance> employeeAttendancesList = await EmployeeAttendanceService.GetAllAsync();
            List<EmployeeAttendance> employeeAttendances = employeeAttendancesList.ToList();
            IEnumerable<Employee> employees = await EmployeeService.GetAllAsync();
            IEnumerable<Department> departments = await DepartmentService.GetAllAsync();
            IEnumerable<Shift> shifts = await ShiftService.GetAllAsync();

            List<Employee> mergedList = employees
            .GroupJoin(
                tuple.Item1, left => left.EmployeeId, right => right.EmployeeId,
                (x, y) => new { Left = x, Rights = y }
            )
            .SelectMany(
                x => x.Rights.DefaultIfEmpty(),
                (x, y) => new Employee
                {
                    EmployeeId = x.Left.EmployeeId,
                    DepartmentId = x.Left.DepartmentId,
                    LastName = x.Left.LastName,
                    FirstName = x.Left.FirstName
                }
            ).ToList();

            if (mergedList != null)
            {
                foreach(Employee employee in mergedList)
                {
                    EmployeeAttendance employeeAttendance = new EmployeeAttendance
                    {
                        EmployeeAttendanceId = 0,
                        EmployeeId = employee.EmployeeId,
                        Employee = employee,
                    };
                    employeeAttendances.Add(employeeAttendance);
                }
            }
            employeeAttendances.AddRange(tuple.Item1);
            var data = employeeAttendances.Select(d => new
            {
                d.EmployeeAttendanceId,
                d.ShiftStart,
                d.ShiftEnd,
                d.BreakStart,
                d.BreakEnd,
                d.EmployeeId,
                EmployeeName = $"{d.Employee!.LastName}, {d.Employee!.FirstName}",
                OrderNumber = orderNumber++
            });
            var dto = new
            {
                Data = data,
                Total = employeeAttendances.Count()
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpGet("AttendanceCutOffFilter")]
        public async Task<IActionResult> EmployeeAttendanceCutOffFilter(int departmentId, DateTime fromDate, int cutOffId)
        {
            CutOff cutOff = await CutOffService.GetAsync(cutOffId);
            if (cutOff == null)
                return BadRequest();

            IEnumerable<EmployeeAttendance> employeeAttendance = await EmployeeAttendanceService.EmployeeAttendanceCutOffFilter(departmentId, cutOff.StartDate, cutOff.EndDate);
            IEnumerable<Employee> employees = await EmployeeService.GetEmployeeByDepartment(departmentId);
            List<EmployeeAttendanceSummary> attendanceList = new List<EmployeeAttendanceSummary>();
            TimekeepingAdminSetup adminSetup = await TimekeepingAdminSetupService.GetFirstOrDefault();
            foreach (var employee in employees)
            {
                var perEmployeeAttendance = employeeAttendance.Where(e => e.EmployeeId == employee.EmployeeId);
                if (perEmployeeAttendance.Any())
                {
                    double shiftTotalHours = 0;
                    double regularTotalHours = 0;
                    double totalLoggedHours = 0;
                    double shiftLateTotalMinutes = 0;
                    double shiftUndertimeTotalMinutes = 0;
                    double breakUndertimeTotalMinutes = 0;
                    double breakLateTotalMinutes = 0;
                    double overtimeTotalHours = 0;
                    double nightDifferentialTotalHours = 0;
                    double holidayTotalHours = 0;
                    double holidayOvertimeTotalHours = 0;
                    double holidayNightDifferentialTotalHours = 0;
                    double specialHolidayTotalHours = 0;
                    double specialHolidayOvertimeTotalHours = 0;
                    double specialHolidayNightDifferentialTotalHours = 0;
                    double restDayTotalHours = 0;
                    double restDayOvertimeTotalHours = 0;
                    double restDayNightDifferentialTotalHours = 0;
                    bool isHoliday = false;
                    bool isRestDay = false;
                    IEnumerable<Holiday> holidays = await HolidayService.GetAllAsync();
                    DaysEnum[] values = (DaysEnum[])Enum.GetValues(typeof(DaysEnum));
                    foreach (var attendance in perEmployeeAttendance)
                    {
                        string[] daysString = values.Select(v => v.ToString()).ToArray();
                        string dayWorked = attendance.TimeIn.DayOfWeek.ToString();
                        int dayIndex = Array.IndexOf(daysString, dayWorked) + 1;
                        int restDays = Array.IndexOf(adminSetup.RestDays!.Split(';', StringSplitOptions.TrimEntries),dayIndex.ToString());
                        isRestDay = restDays < 0;
                        isHoliday = holidays.Any(h => h.HolidayDate!.Value.Day == attendance.TimeIn.Day);
                        shiftTotalHours += attendance.ShiftHours;
                        regularTotalHours += attendance.RegularHour;
                        totalLoggedHours += attendance.TotalLoggedHours;
                        shiftLateTotalMinutes += attendance.ShiftLate;
                        shiftUndertimeTotalMinutes += attendance.ShiftUndertime;
                        breakUndertimeTotalMinutes += attendance.BreakUndertime;
                        breakLateTotalMinutes += attendance.BreakLate;
                        overtimeTotalHours += attendance.ApprovedOT ? attendance.OTHours : 0;
                        nightDifferentialTotalHours += attendance.NDHours;
                        holidayTotalHours += attendance.ApprovedHoliday ? (isHoliday ? attendance.RegularHour : 0) : 0;
                        holidayOvertimeTotalHours += attendance.ApprovedHolidayOT ? (isHoliday ? attendance.OTHours : 0) : 0;
                        holidayNightDifferentialTotalHours += attendance.ApprovedHoliday ? (isHoliday ? attendance.NDHours : 0) : 0;
                        specialHolidayTotalHours += attendance.ApprovedSPHoliday ? (isHoliday ? attendance.RegularHour : 0) : 0;
                        specialHolidayOvertimeTotalHours += attendance.ApprovedSPHolidayOT ? (isHoliday ? attendance.OTHours : 0) : 0;
                        specialHolidayNightDifferentialTotalHours += attendance.ApprovedSPHoliday ? (isHoliday ? attendance.NDHours : 0) : 0;
                        restDayTotalHours += attendance.ApprovedRestDay ? (isRestDay ? attendance.RegularHour : 0) : 0;
                        restDayOvertimeTotalHours += attendance.ApprovedRestDayOT ? (isRestDay ? attendance.OTHours : 0) : 0;
                        restDayNightDifferentialTotalHours += attendance.ApprovedRestDay ? attendance.NDHours : 0;
                    }
                    var finalAttendance = new EmployeeAttendanceSummary
                    {
                        EmployeeNo = employee.EmployeeNo,
                        EmployeeName = employee.LastName + ", " + employee.FirstName,
                        ShiftTotalHours = shiftTotalHours,
                        RegularTotalHours = regularTotalHours,
                        TotalLoggedHours = totalLoggedHours,
                        ShiftLateTotalMinutes = shiftLateTotalMinutes,
                        ShiftUndertimeTotalMinutes = shiftUndertimeTotalMinutes,
                        BreakUndertimeTotalMinutes = breakUndertimeTotalMinutes,
                        BreakLateTotalMinutes = breakLateTotalMinutes,
                        OvertimeTotalHours = overtimeTotalHours,
                        NightDifferentialTotalHours = nightDifferentialTotalHours,
                        HolidayTotalHours = holidayTotalHours,
                        HolidayOvertimeTotalHours = holidayOvertimeTotalHours,
                        HolidayNightDifferentialTotalHours = holidayNightDifferentialTotalHours,
                        SpecialHolidayTotalHours = specialHolidayTotalHours,
                        SpecialHolidayOvertimeTotalHours = specialHolidayOvertimeTotalHours,
                        SpecialHolidayNightDifferentialTotalHours = specialHolidayNightDifferentialTotalHours,
                        RestDayTotalHours = restDayTotalHours,
                        RestDayOvertimeTotalHours = restDayOvertimeTotalHours,
                        RestDayNightDifferentialTotalHours = restDayNightDifferentialTotalHours,
                    };
                    attendanceList.Add(finalAttendance);
                }
            }
            return Ok(attendanceList);
        }

        [Authorize]
        [HttpGet("AttendanceFilter")]
        public async Task<IActionResult> EmployeeAttendanceFilterAsync(int departmentId, DateTime attendanceDate)
        {
            IEnumerable<EmployeeTimeLog> employeeTimeLogs = await EmployeeTimeLogService.GetDailyLogByDeptAsync(departmentId, attendanceDate);
            IEnumerable<EmployeeAttendance>? employeeAttendancesList = null;
            List<EmployeeAttendanceUpdateRequestModel> listToDisplay = new List<EmployeeAttendanceUpdateRequestModel>();
            employeeAttendancesList = await EmployeeAttendanceService.EmployeeAttendanceFilter(departmentId, attendanceDate);

            int[] assignedIds = employeeAttendancesList.Select(x => x.TimeLogId).ToArray();

            List<EmployeeAttendance> employeeAttendances = new List<EmployeeAttendance>();
            IEnumerable<Department> departmentList = await DepartmentService.GetAllAsync();
            List<Department> departments = departmentList.ToList();
            employeeTimeLogs = employeeTimeLogs.Where(item => !assignedIds.Any(x => x == item.TimeLogId)).ToList();
            if (departmentId != 0)
            {
                employeeTimeLogs = employeeTimeLogs.Where(e => e.Employee?.DepartmentId == departmentId);
            }
            List<EmployeeTimeLog> mergedList = employeeTimeLogs
            .GroupJoin(
                employeeAttendances, left => left.EmployeeId, right => right.EmployeeId,
                (x, y) => new { Left = x, Rights = y }
            )
            .SelectMany(
                x => x.Rights.Where(right => x.Left.EmployeeId == right.EmployeeId && x.Left.DateIn != right.TimeIn.Date).DefaultIfEmpty(),
                (x, y) => new EmployeeTimeLog
                {
                    EmployeeId = x.Left.EmployeeId,
                    TimeLogId = x.Left.TimeLogId,
                    TimeIn = x.Left.TimeIn,
                    TimeOut = x.Left.TimeOut,
                    BreakIn = x.Left.BreakIn,
                    BreakOut = x.Left.BreakOut,
                    ShiftStart = x.Left.ShiftStart,
                    ShiftEnd = x.Left.ShiftEnd,
                    BreakStart = x.Left.BreakStart,
                    BreakEnd = x.Left.BreakEnd,
                    IsFlexibleShift = x.Left.IsFlexibleShift,
                    IsFlexibleBreak = x.Left.IsFlexibleBreak,
                    IsNoShift = x.Left.IsNoShift,
                    IsNoBreak = x.Left.IsNoBreak,
                    Employee = x.Left.Employee
                }
            ).ToList();

            if (mergedList != null)
            {
                foreach (EmployeeTimeLog employee in mergedList)
                {
                    var duplicatedLog = employeeAttendancesList.Where(e => e.EmployeeId == employee.EmployeeId && e.TimeIn.Date.ToShortDateString() == employee.TimeIn!.Value.ToShortDateString()).SingleOrDefault();
                    if (duplicatedLog == null)
                    {
                        EmployeeAttendance employeeAttendance = new EmployeeAttendance
                        {
                            EmployeeAttendanceId = 0,
                            EmployeeId = employee.EmployeeId,
                            TimeLogId = employee.TimeLogId,
                            TimeIn = (DateTime)employee.TimeIn!.Value,
                            TimeOut = (DateTime)employee.TimeOut!.Value,
                            BreakIn = employee.IsNoBreak ? null : (DateTime)employee.BreakIn!.Value,
                            BreakOut = employee.IsNoBreak ? null : (DateTime)employee.BreakOut!.Value,
                            ShiftStart = employee.ShiftStart,
                            ShiftEnd = employee.ShiftEnd,
                            BreakStart = employee.IsNoBreak ? null : employee.BreakStart,
                            BreakEnd = employee.IsNoBreak ? null : employee.BreakEnd,
                            IsFlexibleShift = employee.IsFlexibleShift,
                            IsFlexibleBreak = employee.IsFlexibleBreak,
                            IsNoShift = employee.IsNoShift,
                            IsNoBreak = employee.IsNoBreak,
                            Employee = employee.Employee,
                        };
                        employeeAttendances.Add(employeeAttendance);
                    }
                }
            }
            employeeAttendances.AddRange(employeeAttendancesList);
            foreach (EmployeeAttendance employeeAttendance in employeeAttendances.ToList())
            {
                EmployeeAttendanceUpdateRequestModel employeeAttendanceUpdateRequestModel = new EmployeeAttendanceUpdateRequestModel
                {
                    EmployeeAttendanceId = employeeAttendance.EmployeeAttendanceId,
                    TimeLogId = employeeAttendance.TimeLogId,
                    EmployeeId = employeeAttendance.EmployeeId,
                    EmployeeName = $"{employeeAttendance.Employee?.LastName}, {employeeAttendance.Employee?.FirstName}",
                    TimeIn = employeeAttendance.TimeIn,
                    TimeOut = employeeAttendance.TimeOut,
                    BreakIn = employeeAttendance.IsNoBreak ? null : employeeAttendance.BreakIn,
                    BreakOut = employeeAttendance.IsNoBreak ? null : employeeAttendance.BreakOut,
                    ShiftStart = employeeAttendance.ShiftStart,
                    ShiftEnd = employeeAttendance.ShiftEnd,
                    BreakStart= employeeAttendance.IsNoBreak ? null : employeeAttendance.BreakStart,
                    BreakEnd= employeeAttendance.IsNoBreak ? null : employeeAttendance.BreakEnd,
                    IsFlexibleShift = employeeAttendance.IsFlexibleShift,
                    IsFlexibleBreak = employeeAttendance.IsFlexibleBreak,
                    IsNoShift = employeeAttendance.IsNoShift,
                    IsNoBreak = employeeAttendance.IsNoBreak,
                    ShiftHours = employeeAttendance.ShiftHours,
                    RegularHour = employeeAttendance.RegularHour,
                    TotalLoggedHours = employeeAttendance.TotalLoggedHours,
                    ApprovedOT = employeeAttendance.ApprovedOT,
                    OTHours = employeeAttendance.OTHours,
                    NDHours = employeeAttendance.NDHours,
                    ShiftUndertime = employeeAttendance.ShiftUndertime,
                    ShiftLate = employeeAttendance.ShiftLate,
                    BreakUndertime = employeeAttendance.BreakUndertime,
                    BreakLate = employeeAttendance.BreakLate,
                    ApprovedHoliday = employeeAttendance.ApprovedHoliday,
                    ApprovedHolidayOT = employeeAttendance.ApprovedHolidayOT,
                    ApprovedSPHoliday = employeeAttendance.ApprovedSPHoliday,
                    ApprovedSPHolidayOT = employeeAttendance.ApprovedSPHolidayOT,
                    ApprovedRestDay = employeeAttendance.ApprovedRestDay,
                    ApprovedRestDayOT = employeeAttendance.ApprovedRestDayOT,
                    Employee = employeeAttendance.Employee,
                    Department = departmentList.Where(d => d.DepartmentId == employeeAttendance.Employee?.DepartmentId).Select(d => d.DepartmentName).FirstOrDefault(),
                };
                listToDisplay.Add(employeeAttendanceUpdateRequestModel);
            }
          
            var data = listToDisplay.Select(d => new
            {
                d.EmployeeAttendanceId,
                d.TimeLogId,
                d.EmployeeId,
                EmployeeName = $"{d.Employee?.LastName}, {d.Employee?.FirstName}",
                d.TimeIn,
                d.TimeOut,
                d.BreakIn,
                d.BreakOut,
                d.ShiftStart,
                d.ShiftEnd,
                d.BreakStart,
                d.BreakEnd,
                d.IsFlexibleShift,
                d.IsFlexibleBreak,
                d.IsNoShift,
                d.IsNoBreak,
                d.ShiftHours,
                d.RegularHour,
                d.TotalLoggedHours,
                d.ApprovedOT,
                d.OTHours,
                d.NDHours,
                d.ShiftUndertime,
                d.ShiftLate,
                d.BreakUndertime,
                d.BreakLate,
                d.ApprovedHoliday,
                d.ApprovedHolidayOT,
                d.ApprovedSPHoliday,
                d.ApprovedSPHolidayOT,
                d.ApprovedRestDay,
                d.ApprovedRestDayOT,
                d.Department,
            });

            return Ok(data);
        }

        [Authorize]
        [HttpPost("Compute")]
        public async Task<IActionResult> UpdateShiftAssignmentAsync(List<EmployeeAttendanceUpdateRequestModel> employeeAttendance)
        {
            if (employeeAttendance.Count() == 0 || employeeAttendance == null)
                return BadRequest();

            //TimekeepingAdminSetup timekeepingAdminSetup = await TimekeepingAdminSetupService.GetAsync(2);
            TimekeepingAdminSetup timekeepingAdminSetup = await TimekeepingAdminSetupService.GetFirstOrDefault();

            foreach (var item in employeeAttendance)
            {
                DateTime ndStart = Convert.ToDateTime(item.TimeIn.Date.ToShortDateString() + " 10:00 PM");
                DateTime ndEnd = Convert.ToDateTime(item.TimeIn.Date.AddDays(1).ToShortDateString() + " 06:00 AM");
                DateTime ndLogStart = new DateTime();
                DateTime ndLogEnd = new DateTime();
                bool withND = false;
                double shiftLateGracePeriod = Convert.ToDouble(timekeepingAdminSetup.ShiftLateMinuteGracePeriod) / 60;
                if (item.TimeIn >= ndStart && item.TimeIn <= ndEnd)
                {
                    ndLogStart = item.TimeIn;
                    withND=true;
                }
                else if (item.TimeIn < ndStart && item.TimeOut >= ndStart || item.TimeIn < ndStart && item.TimeOut >= ndEnd)
                {
                    ndLogStart = ndStart;
                    withND = true;
                }
                else
                {
                    withND = false;
                }
                if (withND == true && item.TimeOut >= ndStart && item.TimeOut <= ndEnd)
                {
                    ndLogEnd = item.TimeOut;
                    withND = true;
                }
                else if (withND == true && item.TimeOut > ndEnd)
                {
                    ndLogEnd = ndEnd;
                    withND = true;
                }
                else
                { 
                    withND = false; 
                }
                EmployeeAttendance attendance = new EmployeeAttendance
                {
                    EmployeeAttendanceId = item.EmployeeAttendanceId,
                    TimeLogId = item.TimeLogId,
                    EmployeeId = item.EmployeeId,
                    TimeIn = item.TimeIn,
                    TimeOut = item.TimeOut,
                    BreakIn = item.IsNoBreak ? null : item.BreakIn,
                    BreakOut = item.IsNoBreak ? null : item.BreakOut,
                    ShiftStart = item.ShiftStart,
                    ShiftEnd = item.ShiftEnd,
                    BreakStart = item.IsNoBreak ? null : item.BreakStart,
                    BreakEnd = item.IsNoBreak ? null : item.BreakEnd,
                    IsFlexibleShift = item.IsFlexibleShift,
                    IsFlexibleBreak = item.IsFlexibleBreak,
                    IsNoShift = item.IsNoShift,
                    IsNoBreak = item.IsNoBreak,
                    ShiftHours = ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value),
                    RegularHour = ComputeHours(item.TimeIn, item.TimeOut) > ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value) ? ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value) : ComputeHours(item.TimeIn, item.TimeOut),
                    TotalLoggedHours = ComputeHours(item.TimeIn, item.TimeOut),
                    ApprovedOT = item.ApprovedOT,
                    OTHours = item.ApprovedOT == true ? ComputeHours(item.TimeIn, item.TimeOut) - ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value) : 0,
                    NDHours = withND == true ? ComputeHours(ndLogStart, ndLogEnd) : 0,
                    ShiftUndertime = item.TimeOut < item.ShiftEnd ? ComputeHours(item.TimeOut, item.ShiftEnd.Value) : 0,
                    ShiftLate = item.TimeIn > item.ShiftStart ? (ComputeHours(item.ShiftStart.Value, item.TimeIn) > shiftLateGracePeriod ? ComputeHours(item.ShiftStart.Value, item.TimeIn) : 0) : 0,
                    BreakUndertime = item.IsNoBreak ? 0 : ComputeHours(item.BreakOut!.Value, item.BreakStart!.Value),
                    BreakLate = item.IsNoBreak ? 0 : ComputeHours(item.BreakOut!.Value, item.BreakEnd!.Value.AddMinutes(timekeepingAdminSetup.BreakLateMinuteGracePeriod)),
                    ApprovedHoliday = item.ApprovedHoliday,
                    ApprovedHolidayOT = item.ApprovedHolidayOT,
                    ApprovedSPHoliday = item.ApprovedSPHoliday,
                    ApprovedSPHolidayOT = item.ApprovedSPHolidayOT,
                    ApprovedRestDay = item.ApprovedRestDay,
                    ApprovedRestDayOT = item.ApprovedRestDayOT,
                    Employee = item.Employee,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "manuel"
                };
                if (item.EmployeeAttendanceId == 0)
                    await EmployeeAttendanceService.InsertAsync(attendance);
                else
                    await EmployeeAttendanceService.UpdateAsync(attendance);
            }
            return StatusCode(201, employeeAttendance);
        }

        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(EmployeeAttendance employeeAttendance)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            await EmployeeAttendanceService.UpdateAsync(employeeAttendance);

            return Ok();
        }

        //[Authorize]
        //[HttpDelete("{employeeAttendanceId}")]
        //public async Task<IActionResult> RemoveAsync(int employeeAttendanceId)
        //{
        //    if (!ModelState.IsValid)
        //        return BadRequest("Bad Request.");

        //    var deleted = await EmployeeAttendanceService.DeleteAsync(employeeAttendanceId);

        //    if (!deleted)
        //        return NotFound(ResponseMessage.NotFound);

        //    return Ok();
        //}
        private static double ComputeHours(DateTime start, DateTime end)
        {
            TimeSpan minutes = end - start;
            return minutes.TotalMinutes / 60;
        }
    }
}
