using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.Operations;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class EmployeeTimeLogController : ControllerBase
    {
        private IEmployeeTimeLogService EmployeeTimeLogService { get; }
        private IBiometricsLogService BiometricsLogService { get; }
        private IXEmployeeService EmployeeService { get; }
        private IEmployeeShiftService EmployeeShiftService { get; }
        private IXCompanyBranchService CompanyBranchService { get; }

        public EmployeeTimeLogController(IEmployeeTimeLogService employeeTimeLogService, IBiometricsLogService biometricsLogService, IXEmployeeService employeeService, IEmployeeShiftService employeeShiftService, IXCompanyBranchService companyBranchService)
        {
            EmployeeTimeLogService = employeeTimeLogService;
            BiometricsLogService = biometricsLogService;
            EmployeeService = employeeService;
            EmployeeShiftService = employeeShiftService;
            CompanyBranchService = companyBranchService;
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

        [HttpGet("FilterPerProject")]
        public async Task<IActionResult> FilterPerProjectAndDateRange(DateTime? startDate, DateTime? endDate, string? projectName)
        {
            var tuple = await EmployeeTimeLogService.FilterByProjectAndDateRange(startDate, endDate, projectName);

            var data = tuple.Select(d => new
            {
                d.TimeLogId,
                d.EmployeeId,
                employeeNo = d.Employee?.EmployeeId.ToString(),
                EmployeeName = d.Employee?.LastName + "," + d.Employee?.FirstName,
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
            IEnumerable<BiometricsLog> biometricsLogs = await BiometricsLogService.FilterByProjectAndDateRange(startImportDate, endImportDate, projectName);
            List<string> bioEmployees = new List<string>();
            List<string?> bioDates = new List<string?>();
            var projects = await CompanyBranchService.GetAllAsync();
            int projectId = projects.Where(p => p.Name?.ToUpper() == projectName?.ToUpper()).Select(p => p.Id).FirstOrDefault();
            bioDates = biometricsLogs.Select(d => d.Date.ToString()).Distinct().ToList(); //tuple.Item1.Select(d => d.Date.ToString()).Distinct().ToList();
            bioEmployees = biometricsLogs.Select(static d => d.PersonnelId).Distinct().ToList()!;// tuple.Item1.Select(static d => d.PersonnelId).Distinct().ToList();
            IEnumerable<XEmployee> employees = await EmployeeService.GetEmployeeByProject(projectId);
            IEnumerable<EmployeeShift> shifts = await EmployeeShiftService.GetAllAsync();
            var filteredEmployees = from e in employees join b in bioEmployees on e.Id.ToString() equals b select e;
            List<EmployeeTimeLog> timeLogs = new List<EmployeeTimeLog>();
            foreach (var employee in filteredEmployees)
            {
                EmployeeShift? shift = shifts.Where(s => s.EmployeeId == employee.Id).FirstOrDefault();
                if (shift != null)
                {
                    foreach (var date in bioDates)
                    {
                        BiometricsLog biometricsLog = new BiometricsLog();
                        EmployeeTimeLog employeeTimeLog = new EmployeeTimeLog();
                        employeeTimeLog.EmployeeId = employee.Id;
                        employeeTimeLog.DateIn = Convert.ToDateTime(date);
                        employeeTimeLog.DateOut = Convert.ToDateTime(date);
                        biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date?.ToShortDateString() == Convert.ToDateTime(date).ToShortDateString()).OrderBy(e => e.Date).FirstOrDefault();
                        employeeTimeLog.TimeIn = biometricsLog!.Time;
                        employeeTimeLog.ProjecTimeIn = biometricsLog.ProjectName;
                        employeeTimeLog.DeviceTimeIn = biometricsLog.DeviceName;
                        if (employeeTimeLog.DateIn.Value.Day == 1)
                        {
                            if (shift!.MondayShiftEnd < shift.MondayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1) + " " + shift.MondayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog!.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog!.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.MondayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.MondayShiftEnd > shift.MondayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.MondayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.MondayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.Day == 2)
                        {
                            if (shift!.TuesdayShiftEnd < shift.TuesdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1) + " " + shift.TuesdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.TuesdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.TuesdayShiftEnd > shift.TuesdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.TuesdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.TuesdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.Day == 3)
                        {
                            if (shift!.WednesdayShiftEnd < shift.WednesdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1) + " " + shift.WednesdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.WednesdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.WednesdayShiftEnd > shift.WednesdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.WednesdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.WednesdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.Day == 4)
                        {
                            if (shift!.ThursdayShiftEnd < shift.ThursdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1) + " " + shift.ThursdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.ThursdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.ThursdayShiftEnd > shift.ThursdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.ThursdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.ThursdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.Day == 5)
                        {
                            if (shift!.FridayShiftEnd < shift.FridayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1) + " " + shift.FridayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.FridayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.FridayShiftEnd > shift.FridayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.FridayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.FridayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.Day == 6)
                        {
                            if (shift!.SaturdayShiftEnd < shift.SaturdayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1) + " " + shift.SaturdayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            employeeTimeLog.ShiftStart = Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.SaturdayShiftStart!.Value.ToShortTimeString());
                            employeeTimeLog.ShiftEnd = shift.SaturdayShiftEnd > shift.SaturdayShiftStart ? Convert.ToDateTime(Convert.ToDateTime(date).ToShortDateString() + " " + shift.SaturdayShiftEnd!.Value.ToShortTimeString()) : Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1).ToShortDateString() + " " + shift.SaturdayShiftEnd!.Value.ToShortTimeString());
                        }
                        if (employeeTimeLog.DateIn.Value.Day == 7)
                        {
                            if (shift!.SundayShiftEnd < shift.SundayShiftStart)
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date < Convert.ToDateTime(Convert.ToDateTime(date).AddDays(1) + " " + shift.SundayShiftStart!.Value.ToShortTimeString())).OrderBy(e => e.Date).LastOrDefault();
                                employeeTimeLog.TimeOut = biometricsLog!.Time;
                                employeeTimeLog.ProjectTimeOut = biometricsLog.ProjectName;
                                employeeTimeLog.DeviceTimeOut = biometricsLog!.DeviceName;
                            }
                            else
                            {
                                biometricsLog = biometricsLogs.Where(i => i.PersonnelId == employee.Id.ToString() && i.Date.ToString() == date).OrderBy(e => e.Date).LastOrDefault();
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
                d.ProjecTimeIn,
                d.ProjectTimeOut,
                d.DeviceTimeIn,
                d.DeviceTimeOut,
                d.SystemRemarks,
                d.CreatedAt
            });
            return Ok(displayData);
        }

        [HttpPost()]
        public async Task<IActionResult> InsertAsync(EmployeeTimeLog employeeTimeLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            if (employeeTimeLog.TimeIn == null && employeeTimeLog.TimeOut == null)
                return BadRequest("Bad Request.");
            var hasDuplicate = await EmployeeTimeLogService.HasDuplicateName(employeeTimeLog);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);

            if (employeeTimeLog.TimeOut < employeeTimeLog.TimeIn && employeeTimeLog.DateIn == employeeTimeLog.DateOut)
            {
                employeeTimeLog.DateOut = employeeTimeLog.DateOut!.Value.AddDays(1);
                employeeTimeLog.TimeOut = employeeTimeLog.TimeOut.Value.AddDays(1);
            }
            if (employeeTimeLog.ShiftStart > employeeTimeLog.ShiftEnd)
            {
                employeeTimeLog.ShiftEnd = employeeTimeLog.ShiftEnd!.Value.AddDays(1);
            }
            employeeTimeLog.SystemRemarks = "Manual Add";
            employeeTimeLog.CreatedAt = DateTime.UtcNow;
            employeeTimeLog.CreatedBy = "manuel";
            await EmployeeTimeLogService.InsertAsync(employeeTimeLog);

            return StatusCode(201, employeeTimeLog.TimeLogId);
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(EmployeeTimeLog employeeTimeLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            if (employeeTimeLog.TimeOut < employeeTimeLog.TimeIn && employeeTimeLog.DateIn == employeeTimeLog.DateOut)
            {
                employeeTimeLog.DateOut = employeeTimeLog.DateOut!.Value.AddDays(1);
                employeeTimeLog.TimeOut = employeeTimeLog.TimeOut.Value.AddDays(1);
            }
            if (employeeTimeLog.ShiftStart > employeeTimeLog.ShiftEnd)
            {
                employeeTimeLog.ShiftEnd = employeeTimeLog.ShiftEnd!.Value.AddDays(1);
            }
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
