using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Service.Interfaces;

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
        private IEmployeeShiftService EmployeeShiftService { get; }
        public EmployeeAttendanceController(IEmployeeAttendanceService employeeAttendanceService, IEmployeeService employeeService, IShiftService shiftService, IDepartmentService departmentService, IEmployeeTimeLogService employeeTimeLogService, IEmployeeShiftService employeeShiftService)
        {
            EmployeeAttendanceService = employeeAttendanceService;
            EmployeeService = employeeService;
            ShiftService = shiftService;
            DepartmentService = departmentService;
            EmployeeTimeLogService = employeeTimeLogService;
            EmployeeShiftService = employeeShiftService;
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
        public async Task<IActionResult> EmployeeAttendanceCutOffFilter(int projectId, int cutOffId)
        {
            var attendanceList = await EmployeeAttendanceService.GetCutOffSummaryAsync(projectId, cutOffId);

            if (attendanceList == null)
                return BadRequest();

            return Ok(attendanceList);
        }

        [Authorize]
        [HttpGet("AttendanceFilter")]
        public async Task<IActionResult> EmployeeAttendanceFilterAsync(int projectId, DateTime attendanceDate)
        {
            IEnumerable<EmployeeTimeLog> employeeTimeLogs = await EmployeeTimeLogService.GetDailyLogByProjectAsync(projectId, attendanceDate);
            IEnumerable<EmployeeAttendance>? employeeAttendancesList = null;
            List<EmployeeAttendanceUpdateRequestModel> listToDisplay = new List<EmployeeAttendanceUpdateRequestModel>();
            employeeAttendancesList = await EmployeeAttendanceService.EmployeeAttendanceFilter(projectId, attendanceDate);

            int[] assignedIds = employeeAttendancesList.Select(x => x.TimeLogId).ToArray();

            List<EmployeeAttendance> employeeAttendances = new List<EmployeeAttendance>();
            IEnumerable<Department> departmentList = await DepartmentService.GetAllAsync();
            List<Department> departments = departmentList.ToList();
            employeeTimeLogs = employeeTimeLogs.Where(item => !assignedIds.Any(x => x == item.TimeLogId)).ToList();
            if (projectId != 0)
            {
                employeeTimeLogs = employeeTimeLogs.Where(e => e.Employee?.ProjectId == projectId);
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
                    ShiftStart = x.Left.ShiftStart,
                    ShiftEnd = x.Left.ShiftEnd,
                    IsFlexibleShift = x.Left.IsFlexibleShift,
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
                            ShiftStart = employee.ShiftStart,
                            ShiftEnd = employee.ShiftEnd,
                            IsFlexibleShift = employee.IsFlexibleShift,
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
                    ShiftStart = employeeAttendance.ShiftStart,
                    ShiftEnd = employeeAttendance.ShiftEnd,
                    IsFlexibleShift = employeeAttendance.IsFlexibleShift,
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
                d.ShiftStart,
                d.ShiftEnd,
                d.IsFlexibleShift,
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

            //TimekeepingAdminSetup timekeepingAdminSetup = await TimekeepingAdminSetupService.GetFirstOrDefault();

            foreach (var item in employeeAttendance)
            {
                DateTime ndStart = Convert.ToDateTime(item.TimeIn.Date.ToShortDateString() + " 10:00 PM");
                DateTime ndEnd = Convert.ToDateTime(item.TimeIn.Date.AddDays(1).ToShortDateString() + " 06:00 AM");
                DateTime ndLogStart = new DateTime();
                DateTime ndLogEnd = new DateTime();
                bool withND = false;
                double shiftLateGracePeriod = 0;
                EmployeeShift? employeeShift = await EmployeeShiftService.GetAtAsync(item.EmployeeId, DateTime.SpecifyKind(item.TimeIn, DateTimeKind.Unspecified));
                if (employeeShift != null)
                {
                    Shift shift = await ShiftService.GetAsync(employeeShift.ShiftId);
                    if (shift != null)
                    {
                        shiftLateGracePeriod = Convert.ToDouble(shift.ShiftLateMinuteGracePeriod) / 60;
                    }
                }
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
                    ShiftStart = item.ShiftStart,
                    ShiftEnd = item.ShiftEnd,
                    IsFlexibleShift = item.IsFlexibleShift,
                    IsNoShift = item.IsNoShift,
                    IsNoBreak = item.IsNoBreak,
                    ShiftHours = ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value),
                    RegularHour = ComputeHours(item.TimeIn, item.TimeOut) > ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value) ? ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value) : ComputeHours(item.TimeIn, item.TimeOut),
                    TotalLoggedHours = ComputeHours(item.TimeIn, item.TimeOut),
                    ApprovedOT = item.ApprovedOT,
                    OTHours = item.ApprovedOT == true ? ComputeHours(item.TimeIn, item.TimeOut) - ComputeHours(item.ShiftStart!.Value, item.ShiftEnd!.Value) : 0,
                    NDHours = withND == true ? ComputeHours(ndLogStart, ndLogEnd) : 0,
                    ShiftUndertime = item.TimeOut < item.ShiftEnd ? ComputeHours(item.TimeOut, item.ShiftEnd.Value) * 60 : 0,
                    ShiftLate = item.TimeIn > item.ShiftStart ? (ComputeHours(item.ShiftStart.Value, item.TimeIn) > shiftLateGracePeriod ? ComputeHours(item.ShiftStart.Value, item.TimeIn) * 60 : 0) : 0,
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
