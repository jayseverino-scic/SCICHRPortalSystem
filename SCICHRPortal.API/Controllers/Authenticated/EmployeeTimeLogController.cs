using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class EmployeeTimeLogController : ControllerBase
    {
        private IEmployeeTimeLogService EmployeeTimeLogService { get; }
        private IBiometricsLogService BiometricsLogService { get; }
        private IEmployeeService EmployeeService { get; }
        private IEmployeeShiftService EmployeeShiftService { get; }
        private IProjectService ProjectService { get; }

        public EmployeeTimeLogController(IEmployeeTimeLogService employeeTimeLogService, IBiometricsLogService biometricsLogService, IEmployeeService employeeService, IEmployeeShiftService employeeShiftService, IProjectService projectService)
        {
            EmployeeTimeLogService = employeeTimeLogService;
            BiometricsLogService = biometricsLogService;
            EmployeeService = employeeService;
            EmployeeShiftService = employeeShiftService;
            ProjectService = projectService;
        }

        [HttpGet("EmployeeLookup")]
        public async Task<IActionResult> EmployeeLookupAsync(string? term, int page = 1)
        {
            if (page < 1 || page > 100000 || (term?.Length ?? 0) > 100)
                return BadRequest("Enter a valid search term and page.");
            var trimmed = term?.Trim() ?? "";
            if (trimmed.Length < 2)
                return Ok(new EmployeeTimeLogEmployeeLookupPage());
            return Ok(await EmployeeTimeLogService.SearchEmployeesAsync(trimmed, page));
        }

        [HttpGet("EmployeeLookup/{employeeId:int}")]
        public async Task<IActionResult> EmployeeLookupByIdAsync(int employeeId)
        {
            if (employeeId <= 0)
                return BadRequest("Select a valid employee.");
            var employee = await EmployeeService.GetAsync(employeeId);
            if (employee is null || employee.Deleted)
                return NotFound("The selected employee was not found or is inactive.");
            return Ok(new EmployeeTimeLogEmployeeLookupItem
            {
                EmployeeId = employee.EmployeeId,
                EmployeeNo = employee.EmployeeNo,
                FirstName = employee.FirstName,
                LastName = employee.LastName
            });
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var employeeTimeLogs = await EmployeeTimeLogService.GetAllAsync();
            return Ok(employeeTimeLogs);
        }
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            var tuple = await EmployeeTimeLogService.FilterAsync(pageNumber, pageSize, searchKeyword!, startDate, endDate, deviceName);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.TimeLogId,
                d.EmployeeId,
                employeeNo = d.Employee!.EmployeeId.ToString(),
                EmployeeName = d.Employee!.LastName + "," + d.Employee.FirstName,
                d.DateIn,
                d.DateOut,
                d.TimeIn,
                d.TimeOut,
                d.ShiftStart,
                d.ShiftEnd,
                d.IsFlexibleShift,
                d.IsNoShift,
                d.IsNoBreak,
                d.SystemRemarks,
                d.CreatedAt,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };
            return Ok(dto);
        }

        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] EmployeeTimeLogPageQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid || !query.IsValid())
                return BadRequest("Enter valid paging, search, sort and date filters.");
            return Ok(await EmployeeTimeLogService.GetPageAsync(query, cancellationToken));
        }

        [HttpGet("FilterPerProject")]
        public async Task<IActionResult> FilterPerProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName)
        {
            if (startDate.HasValue && endDate.HasValue && startDate.Value.Date > endDate.Value.Date)
                return BadRequest("Start date must be on or before end date.");

            var tuple = await EmployeeTimeLogService.FilterByProjectAndDateRange(startDate, endDate, projectName);

            var data = tuple.Select(d => new
            {
                d.TimeLogId,
                d.EmployeeId,
                employeeNo = d.Employee?.EmployeeId.ToString(),
                EmployeeName = d.Employee?.LastName + "," + d.Employee?.FirstName,
                d.DateIn,
                d.ProjectTimeIn,
                d.DeviceTimeIn,
                d.DateOut,
                d.ProjectTimeOut,
                d.DeviceTimeOut,
                d.TimeIn,
                d.TimeOut,
                d.ShiftStart,
                d.ShiftEnd,
                d.IsFlexibleShift,
                d.IsNoShift,
                d.IsNoBreak,
                d.SystemRemarks,
                d.CreatedAt,
            });

            var dto = new
            {
                Data = data,
                Total = data.Count()
            };
            return Ok(dto);
        }


        [HttpPost("Import")]
        public async Task<IActionResult> ImportAsync(DateTime? startImportDate, DateTime? endImportDate, string? projectName)
        {
            if (!startImportDate.HasValue || !endImportDate.HasValue || string.IsNullOrWhiteSpace(projectName))
                return BadRequest("Select a project and both dates before getting imported logs.");
            if (startImportDate > endImportDate)
                return BadRequest("Start date must be on or before end date.");

            IEnumerable<BiometricsLog> biometricsLogs = await BiometricsLogService.FilterByProjectAndDateRange(startImportDate, endImportDate, projectName);
            List<string> bioEmployees = new List<string>();
            List<string?> bioDates = new List<string?>();
            var projects = await ProjectService.GetAllAsync();
            int projectId = projects.Where(p => p.Name?.ToUpper() == projectName?.ToUpper()).Select(p => p.Id).FirstOrDefault();
            bioDates = biometricsLogs.Select(d => d.Date.ToString()).Distinct().ToList(); //tuple.Item1.Select(d => d.Date.ToString()).Distinct().ToList();
            bioEmployees = biometricsLogs.Select(static d => d.PersonnelId).Distinct().ToList()!;// tuple.Item1.Select(static d => d.PersonnelId).Distinct().ToList();
            IEnumerable<Employee> employees = await EmployeeService.GetEmployeeByProject(projectId);
            IEnumerable<EmployeeShift> shifts = await EmployeeShiftService.GetAllAsync();
            var filteredEmployees = from e in employees join b in bioEmployees on e.EmployeeNo equals b select e;
            List<EmployeeTimeLog> timeLogs = new List<EmployeeTimeLog>();
            foreach (var employee in filteredEmployees)
            {
                EmployeeShift? shift = shifts.Where(s => s.EmployeeId == employee.EmployeeId).FirstOrDefault();
                if (shift != null)
                {
                    foreach (var date in bioDates)
                    {
                        BiometricsLog biometricsLog = new BiometricsLog();
                        EmployeeTimeLog employeeTimeLog = new EmployeeTimeLog();
                        employeeTimeLog.EmployeeId = employee.EmployeeId;
                        employeeTimeLog.DateIn = Convert.ToDateTime(date);
                        employeeTimeLog.DateOut = Convert.ToDateTime(date);
                        biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date?.ToShortDateString() == Convert.ToDateTime(date).ToShortDateString()).OrderBy(e => e.Date).FirstOrDefault();
                        employeeTimeLog.TimeIn = biometricsLog!.Time;
                        employeeTimeLog.ProjectTimeIn = biometricsLog.ProjectName;
                        employeeTimeLog.DeviceTimeIn = biometricsLog.DeviceName;
                        if (employeeTimeLog.DateIn.Value.DayOfWeek.ToString() == "Monday")
                        {
                            if (shift!.MondayShiftEnd < shift.MondayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.MondayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog!.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog!.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.MondayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.MondayShiftEnd > shift.MondayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.MondayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.MondayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.DayOfWeek.ToString() == "Tuesday")
                        {
                            if (shift!.TuesdayShiftEnd < shift.TuesdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.TuesdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.TuesdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.TuesdayShiftEnd > shift.TuesdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.TuesdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.TuesdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.DayOfWeek.ToString() == "Wednesday")
                        {
                            if (shift!.WednesdayShiftEnd < shift.WednesdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.WednesdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.WednesdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.WednesdayShiftEnd > shift.WednesdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.WednesdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.WednesdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.DayOfWeek.ToString() == "Thursday")
                        {
                            if (shift!.ThursdayShiftEnd < shift.ThursdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.ThursdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.ThursdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.ThursdayShiftEnd > shift.ThursdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.ThursdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.ThursdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.DayOfWeek.ToString() == "Friday")
                        {
                            if (shift!.FridayShiftEnd < shift.FridayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.FridayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.FridayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.FridayShiftEnd > shift.FridayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.FridayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.FridayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.DayOfWeek.ToString() == "Saturday")
                        {
                            if (shift!.SaturdayShiftEnd < shift.SaturdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.SaturdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.SaturdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.SaturdayShiftEnd > shift.SaturdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.SaturdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.SaturdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.DayOfWeek.ToString() == "Sunday")
                        {
                            if (shift!.SundayShiftEnd < shift.SundayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.SundayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.EmployeeNo!.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.SundayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.SundayShiftEnd > shift.SundayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.SundayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.SundayShiftEnd!.Value.ToShortTimeString());
                        }
                        employeeTimeLog.IsNoShift = shift!.IsNoShift;
                        employeeTimeLog.IsNoBreak = shift.IsNoBreak;
                        employeeTimeLog.IsFlexibleShift = shift.IsFlexibleShift;
                        employeeTimeLog.SystemRemarks = "Biometrics";
                        employeeTimeLog.CreatedAt = DateTime.UtcNow;
                        employeeTimeLog.CreatedBy = "manuel";
                        timeLogs.Add(employeeTimeLog);
                        await EmployeeTimeLogService.InsertAsync(employeeTimeLog);
                    }
                }
            }
            var displayData = timeLogs.Select(d => new
            {
                d.TimeLogId,
                d.EmployeeId,
                employeeNo = d.Employee!.EmployeeId.ToString(),
                EmployeeName = d.Employee!.LastName + "," + d.Employee.FirstName,
                d.DateIn,
                d.DateOut,
                d.TimeIn,
                d.TimeOut,
                d.ShiftStart,
                d.ShiftEnd,
                d.IsFlexibleShift,
                d.IsNoShift,
                d.IsNoBreak,
                d.ProjectTimeIn,
                d.ProjectTimeOut,
                d.DeviceTimeIn,
                d.DeviceTimeOut,
                d.SystemRemarks,
                d.CreatedAt
            });
            return Ok(displayData);
        }

        [HttpPost()]
        public async Task<IActionResult> InsertAsync(EmployeeTimeLogInsertRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Check the time log fields and try again.");
            if (request.EmployeeId <= 0)
                return BadRequest("Select a valid employee.");
            if (!request.DateIn.HasValue)
                return BadRequest("Date In is required.");
            if (!request.TimeIn.HasValue && !request.TimeOut.HasValue)
                return BadRequest("Enter Time In or Time Out.");
            if (request.TimeOut.HasValue && !request.DateOut.HasValue)
                return BadRequest("Date Out is required when Time Out is entered.");

            var dateIn = request.DateIn.Value.Date;
            var dateOut = request.DateOut?.Date ?? dateIn;
            if (dateOut < dateIn)
                return BadRequest("Date Out cannot be before Date In.");

            var employeeTimeLog = new EmployeeTimeLog
            {
                EmployeeId = request.EmployeeId,
                DateIn = dateIn,
                DateOut = dateOut,
                TimeIn = request.TimeIn.HasValue ? dateIn.Add(request.TimeIn.Value.TimeOfDay) : null,
                TimeOut = request.TimeOut.HasValue ? dateOut.Add(request.TimeOut.Value.TimeOfDay) : null,
                ProjectTimeIn = request.ProjectTimeIn,
                ProjectTimeOut = request.ProjectTimeOut,
                DeviceTimeIn = request.DeviceTimeIn,
                DeviceTimeOut = request.DeviceTimeOut
            };
            var scheduleError = await ApplyAssignedScheduleAsync(employeeTimeLog);
            if (scheduleError is not null)
                return scheduleError;

            if (employeeTimeLog.TimeOut < employeeTimeLog.TimeIn && dateOut == dateIn)
            {
                employeeTimeLog.DateOut = dateOut.AddDays(1);
                employeeTimeLog.TimeOut = employeeTimeLog.TimeOut.Value.AddDays(1);
            }

            var hasDuplicate = await EmployeeTimeLogService.HasDuplicateName(employeeTimeLog);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);

            employeeTimeLog.SystemRemarks = "Manual Add";
            employeeTimeLog.CreatedAt = DateTime.UtcNow;
            employeeTimeLog.CreatedBy = "manuel";
            await EmployeeTimeLogService.InsertAsync(employeeTimeLog);

            return StatusCode(201, employeeTimeLog.TimeLogId);
        }

        private async Task<IActionResult?> ApplyAssignedScheduleAsync(EmployeeTimeLog employeeTimeLog)
        {
            if (employeeTimeLog.EmployeeId <= 0)
                return BadRequest("Select a valid employee.");
            if (!employeeTimeLog.DateIn.HasValue)
                return BadRequest("Date In is required.");

            var employee = await EmployeeService.GetAsync(employeeTimeLog.EmployeeId);
            if (employee is null || employee.Deleted)
                return NotFound("The selected employee was not found or is inactive.");

            EmployeeShift? shift;
            try
            {
                shift = await EmployeeShiftService.GetByEmployee(employeeTimeLog.EmployeeId);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("more than one element", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Multiple assigned shifts were found for this employee. Resolve the assignment before saving a time log.");
            }

            if (shift is null || shift.Deleted)
                return BadRequest("No assigned shift was found for this employee.");
            var dateIn = employeeTimeLog.DateIn.Value.Date;
            var (weekdayStart, weekdayEnd) = GetWeekdayShiftTimes(shift, dateIn.DayOfWeek);
            if (!weekdayStart.HasValue || !weekdayEnd.HasValue)
                return BadRequest($"The assigned shift has incomplete {dateIn.DayOfWeek} times.");

            var shiftStart = dateIn.Add(weekdayStart.Value.TimeOfDay);
            var shiftEnd = dateIn.Add(weekdayEnd.Value.TimeOfDay);
            employeeTimeLog.ShiftStart = shiftStart;
            employeeTimeLog.ShiftEnd = shiftEnd < shiftStart ? shiftEnd.AddDays(1) : shiftEnd;
            employeeTimeLog.IsFlexibleShift = shift.IsFlexibleShift;
            employeeTimeLog.IsNoShift = shift.IsNoShift;
            employeeTimeLog.IsNoBreak = shift.IsNoBreak;
            return null;
        }

        private static (DateTime? Start, DateTime? End) GetWeekdayShiftTimes(EmployeeShift shift, DayOfWeek weekday) => weekday switch
        {
            DayOfWeek.Monday => (shift.MondayShiftStart, shift.MondayShiftEnd),
            DayOfWeek.Tuesday => (shift.TuesdayShiftStart, shift.TuesdayShiftEnd),
            DayOfWeek.Wednesday => (shift.WednesdayShiftStart, shift.WednesdayShiftEnd),
            DayOfWeek.Thursday => (shift.ThursdayShiftStart, shift.ThursdayShiftEnd),
            DayOfWeek.Friday => (shift.FridayShiftStart, shift.FridayShiftEnd),
            DayOfWeek.Saturday => (shift.SaturdayShiftStart, shift.SaturdayShiftEnd),
            DayOfWeek.Sunday => (shift.SundayShiftStart, shift.SundayShiftEnd),
            _ => (null, null)
        };

        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(EmployeeTimeLog employeeTimeLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            var persisted = await EmployeeTimeLogService.GetAsync(employeeTimeLog.TimeLogId);
            if (persisted is null)
                return NotFound(ResponseMessage.NotFound);

            if (persisted.EmployeeId != employeeTimeLog.EmployeeId || persisted.DateIn?.Date != employeeTimeLog.DateIn?.Date)
            {
                var scheduleError = await ApplyAssignedScheduleAsync(employeeTimeLog);
                if (scheduleError is not null)
                    return scheduleError;
            }
            else
            {
                employeeTimeLog.ShiftStart = persisted.ShiftStart;
                employeeTimeLog.ShiftEnd = persisted.ShiftEnd;
                employeeTimeLog.IsFlexibleShift = persisted.IsFlexibleShift;
                employeeTimeLog.IsNoShift = persisted.IsNoShift;
                employeeTimeLog.IsNoBreak = persisted.IsNoBreak;
            }

            if (employeeTimeLog.TimeOut < employeeTimeLog.TimeIn && employeeTimeLog.DateIn == employeeTimeLog.DateOut)
            {
                employeeTimeLog.DateOut = employeeTimeLog.DateOut!.Value.AddDays(1);
                employeeTimeLog.TimeOut = employeeTimeLog.TimeOut.Value.AddDays(1);
            }
            var hasDuplicate = await EmployeeTimeLogService.HasDuplicateName(employeeTimeLog);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);

            employeeTimeLog.SystemRemarks = "Manual Edit";
            employeeTimeLog.UpdatedAt = DateTime.Now;
            employeeTimeLog.UpdatedBy = "manuel";
            var updated = await EmployeeTimeLogService.UpdateAsync(employeeTimeLog);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        [HttpDelete("{employeeTimeLogId}")]
        public async Task<IActionResult> DeleteAsync(int employeeTimeLogId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await EmployeeTimeLogService.DeleteAsync(employeeTimeLogId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
