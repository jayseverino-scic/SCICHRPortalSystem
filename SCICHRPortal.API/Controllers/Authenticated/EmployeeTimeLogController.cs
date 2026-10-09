using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Helpers;
using System.Security.Claims;

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
        private string Actor => User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Sid) ?? "Authenticated user";

        [HttpGet("{id:int}")]
        public async Task<IActionResult> DetailsAsync(int id)
        {
            var record = await EmployeeTimeLogService.GetAsync(id);
            return record is null ? NotFound(ResponseMessage.NotFound) : Ok(ToDetails(record));
        }

        private static object ToDetails(EmployeeTimeLog record) => new
        {
            record.TimeLogId, record.EmployeeId, record.DateIn, record.DateOut, record.TimeIn, record.TimeOut,
            record.ProjectTimeIn, record.ProjectTimeOut, record.DeviceTimeIn, record.DeviceTimeOut,
            record.ShiftStart, record.ShiftEnd, record.IsFlexibleShift, record.IsNoShift, record.IsNoBreak,
            record.SystemRemarks, record.IsOB, record.Comment, record.Version,
            Attachment = record.Attachment is null ? null : new
            {
                record.Attachment.TimeLogAttachmentId, record.Attachment.FileName,
                record.Attachment.ContentType, record.Attachment.Size
            }
        };

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

            if (startImportDate.Value.Date <= DateTime.MinValue.Date || endImportDate.Value.Year >= 9999)
                return BadRequest("The import range is outside supported session dates.");
            var from = startImportDate.Value.Date;
            var until = endImportDate.Value.Date;
            // Include surrounding punches so an overnight session can finish
            // outside the requested start-date range without losing checkout.
            var biometricsLogs = (await BiometricsLogService.FilterByProjectAndDateRange(from.AddDays(-1), until.AddDays(2).AddTicks(-1), projectName)).ToList();
            var projects = await ProjectService.GetAllAsync();
            var projectId = projects.Where(row => string.Equals(row.Name, projectName, StringComparison.OrdinalIgnoreCase)).Select(row => row.Id).FirstOrDefault();
            if (projectId <= 0) return BadRequest("The selected project is unavailable.");
            var employees = (await EmployeeService.GetEmployeeByProject(projectId)).ToList();
            var employeeIds = employees.Select(row => row.EmployeeId).ToArray();
            var periods = await EmployeeShiftService.GetPeriodsAsync(employeeIds, from.AddDays(-1), until.AddDays(2));
            var timeLogs = new List<EmployeeTimeLog>();
            var errors = new List<string>();
            foreach (var employee in employees)
            {
                var employeePunches = biometricsLogs.Where(row => row.PersonnelId == employee.EmployeeNo).ToList();
                if (employeePunches.Count == 0) continue;
                try
                {
                    var result = EmployeeShiftSessions.Build(employee.EmployeeId, employeePunches,
                        periods.Where(row => row.EmployeeId == employee.EmployeeId).ToList(), from, until);
                    timeLogs.AddRange(result.Sessions);
                    errors.AddRange(result.Errors);
                }
                catch (InvalidOperationException)
                {
                    errors.Add($"Employee {employee.EmployeeId}: overlapping assignment history; review before importing.");
                }
            }
            if (errors.Count > 0) return BadRequest(new { Message = "Review missing or ambiguous sessions before importing. No records were imported.", Errors = errors });
            var inserted = new List<EmployeeTimeLog>();
            foreach (var row in timeLogs)
            {
                if ((await EmployeeTimeLogService.HasDuplicateName(row)).IsDuplicated) continue;
                row.CreatedAt = DateTime.UtcNow;
                row.CreatedBy = Actor;
                row.Comment = TimeLogChanges.Append(null, Actor, [$"Imported time record ({row.SystemRemarks})"], DateTime.UtcNow);
                await EmployeeTimeLogService.InsertAsync(row);
                inserted.Add(row);
            }
            timeLogs = inserted;
            var displayData = timeLogs.Select(d => new
            {
                d.TimeLogId,
                d.EmployeeId,
                employeeNo = d.EmployeeId.ToString(),
                EmployeeName = employees.Where(row => row.EmployeeId == d.EmployeeId).Select(row => row.LastName + ", " + row.FirstName).FirstOrDefault(),
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
                DeviceTimeOut = TimeLogChanges.Optional(request.DeviceTimeOut),
                IsOB = request.IsOB
            };
            employeeTimeLog.ProjectTimeIn = TimeLogChanges.Optional(employeeTimeLog.ProjectTimeIn);
            employeeTimeLog.ProjectTimeOut = TimeLogChanges.Optional(employeeTimeLog.ProjectTimeOut);
            employeeTimeLog.DeviceTimeIn = TimeLogChanges.Optional(employeeTimeLog.DeviceTimeIn);
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
            employeeTimeLog.CreatedBy = Actor;
            employeeTimeLog.Comment = TimeLogChanges.Append(null, Actor,
                [$"Created time record (Manual Add); OB: {(employeeTimeLog.IsOB ? "Yes" : "No")}"], DateTime.UtcNow);
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
                if (employeeTimeLog.TimeIn.HasValue)
                    shift = await EmployeeShiftService.GetAtAsync(employeeTimeLog.EmployeeId,
                        PhilippineTime.SessionStart(employeeTimeLog.DateIn.Value, employeeTimeLog.TimeIn));
                else
                {
                    var periods = await EmployeeShiftService.GetPeriodsAsync([employeeTimeLog.EmployeeId],
                        employeeTimeLog.DateIn.Value.Date, employeeTimeLog.DateIn.Value.Date.AddDays(1));
                    if (periods.Count != 1) return BadRequest("Enter Time In to identify the effective schedule for this work session.");
                    shift = periods[0];
                }
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("more than one element", StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Multiple assigned shifts were found for this employee. Resolve the assignment before saving a time log.");
            }

            if (shift is null || shift.Deleted)
                return BadRequest("No assigned shift was found for this employee.");
            var dateIn = employeeTimeLog.DateIn.Value.Date;
            var (weekdayStart, weekdayEnd) = GetWeekdayShiftTimes(shift, dateIn.DayOfWeek);
            if (shift.IsNoShift && (employeeTimeLog.TimeIn.HasValue || employeeTimeLog.TimeOut.HasValue))
            {
                employeeTimeLog.ShiftStart = employeeTimeLog.TimeIn ?? employeeTimeLog.TimeOut;
                employeeTimeLog.ShiftEnd = employeeTimeLog.TimeOut ?? employeeTimeLog.TimeIn;
                employeeTimeLog.IsFlexibleShift = false;
                employeeTimeLog.IsNoShift = true;
                employeeTimeLog.IsNoBreak = shift.IsNoBreak;
                return null;
            }
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
        public async Task<IActionResult> UpdateAsync(EmployeeTimeLogUpdateRequestModel request)
        {
            if (!ModelState.IsValid || request.Version is null || request.Version < 0)
                return BadRequest("Reload the record and provide its current version.");
            var persisted = await EmployeeTimeLogService.GetAsync(request.TimeLogId);
            if (persisted is null)
                return NotFound(ResponseMessage.NotFound);
            if (request.Version != persisted.Version)
                return Conflict("This time record changed. Close and reopen it before saving.");

            var employeeTimeLog = new EmployeeTimeLog
            {
                TimeLogId = persisted.TimeLogId, EmployeeId = request.EmployeeId,
                DateIn = request.DateIn, DateOut = request.DateOut, TimeIn = request.TimeIn, TimeOut = request.TimeOut,
                ProjectTimeIn = TimeLogChanges.Optional(request.ProjectTimeIn),
                ProjectTimeOut = TimeLogChanges.Optional(request.ProjectTimeOut),
                DeviceTimeIn = TimeLogChanges.Optional(request.DeviceTimeIn),
                DeviceTimeOut = TimeLogChanges.Optional(request.DeviceTimeOut), IsOB = request.IsOB,
                CreatedAt = persisted.CreatedAt, CreatedBy = persisted.CreatedBy,
                SystemRemarks = persisted.SystemRemarks, Comment = persisted.Comment,
                ShiftStart = persisted.ShiftStart, ShiftEnd = persisted.ShiftEnd,
                IsFlexibleShift = persisted.IsFlexibleShift, IsNoShift = persisted.IsNoShift, IsNoBreak = persisted.IsNoBreak,
                Attachment = persisted.Attachment
            };
            var protectionError = TimeLogChanges.ProtectedFieldError(persisted, employeeTimeLog);
            if (protectionError is not null) return BadRequest(protectionError);
            if (request.EmployeeId <= 0 || !request.DateIn.HasValue)
                return BadRequest("Employee and Date In are required.");
            if (request.TimeOut.HasValue && !request.DateOut.HasValue)
                return BadRequest("Date Out is required when Time Out is entered.");
            if (!request.TimeIn.HasValue && !request.TimeOut.HasValue)
                return BadRequest("Enter Time In or Time Out.");
            if (request.DateOut?.Date < request.DateIn.Value.Date)
                return BadRequest("Date Out cannot be before Date In.");

            if (persisted.EmployeeId != employeeTimeLog.EmployeeId || persisted.DateIn?.Date != employeeTimeLog.DateIn?.Date)
            {
                var scheduleError = await ApplyAssignedScheduleAsync(employeeTimeLog);
                if (scheduleError is not null)
                    return scheduleError;
            }
            var isProtected = TimeLogChanges.IsProtected(persisted);
            if (!isProtected || !persisted.DateIn.HasValue)
                employeeTimeLog.DateIn = request.DateIn.Value.Date;
            if (!isProtected || !persisted.DateOut.HasValue)
                employeeTimeLog.DateOut = request.DateOut?.Date ?? employeeTimeLog.DateIn;
            if (employeeTimeLog.TimeIn.HasValue && (!isProtected || !persisted.TimeIn.HasValue))
                employeeTimeLog.TimeIn = employeeTimeLog.DateIn!.Value.Date.Add(employeeTimeLog.TimeIn.Value.TimeOfDay);
            if (employeeTimeLog.TimeOut.HasValue && (!isProtected || !persisted.TimeOut.HasValue))
                employeeTimeLog.TimeOut = employeeTimeLog.DateOut!.Value.Date.Add(employeeTimeLog.TimeOut.Value.TimeOfDay);
            if (!isProtected && employeeTimeLog.TimeOut < employeeTimeLog.TimeIn && employeeTimeLog.DateIn == employeeTimeLog.DateOut)
            {
                employeeTimeLog.DateOut = employeeTimeLog.DateOut!.Value.AddDays(1);
                employeeTimeLog.TimeOut = employeeTimeLog.TimeOut.Value.AddDays(1);
            }
            // Date normalization and overnight handling may not alter populated imported values either.
            protectionError = TimeLogChanges.ProtectedFieldError(persisted, employeeTimeLog);
            if (protectionError is not null) return BadRequest(protectionError);
            var changes = TimeLogChanges.Describe(persisted, employeeTimeLog);
            if (changes.Count == 0) return Ok(ToDetails(persisted));
            var identityChanged = persisted.EmployeeId != employeeTimeLog.EmployeeId ||
                persisted.DateIn != employeeTimeLog.DateIn || persisted.DateOut != employeeTimeLog.DateOut ||
                persisted.TimeIn != employeeTimeLog.TimeIn || persisted.TimeOut != employeeTimeLog.TimeOut;
            if (identityChanged)
            {
                var hasDuplicate = await EmployeeTimeLogService.HasDuplicateName(employeeTimeLog);
                if (hasDuplicate.IsDuplicated) return Conflict(hasDuplicate);
            }

            if (!isProtected) employeeTimeLog.SystemRemarks = "Manual Edit";
            employeeTimeLog.Comment = TimeLogChanges.Append(persisted.Comment, Actor, changes, DateTime.UtcNow);
            employeeTimeLog.UpdatedAt = DateTime.UtcNow;
            employeeTimeLog.UpdatedBy = Actor;
            employeeTimeLog.Version = persisted.Version + 1;
            return await SaveRecordAsync(employeeTimeLog);
        }

        [HttpDelete("{employeeTimeLogId}")]
        public async Task<IActionResult> DeleteAsync(int employeeTimeLogId, [FromQuery] long? version)
        {
            if (!ModelState.IsValid || version is null) return BadRequest("Reload the record before deleting it.");
            var record = await EmployeeTimeLogService.GetAsync(employeeTimeLogId);
            if (record is null) return NotFound(ResponseMessage.NotFound);
            if (version != record.Version) return Conflict("This time record changed. Refresh before deleting.");
            record.Deleted = true;
            record.Comment = TimeLogChanges.Append(record.Comment, Actor, ["Deleted time record"], DateTime.UtcNow);
            record.UpdatedAt = DateTime.UtcNow;
            record.UpdatedBy = Actor;
            record.Version++;
            return await SaveRecordAsync(record);
        }

        private async Task<IActionResult> SaveRecordAsync(EmployeeTimeLog record)
        {
            try
            {
                return await EmployeeTimeLogService.UpdateAsync(record)
                    ? Ok(ToDetails(record)) : NotFound(ResponseMessage.NotFound);
            }
            catch (DbUpdateConcurrencyException)
            {
                return Conflict("This time record changed. Close and reopen it before saving.");
            }
        }
    }
}
