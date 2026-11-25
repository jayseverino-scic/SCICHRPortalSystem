using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Helpers;
using System.Globalization;
using System.Security.Claims;

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
        private IEmployeeDailyAttendanceComputationService DailyAttendanceComputationService { get; }

        public EmployeeAttendanceController(IEmployeeAttendanceService employeeAttendanceService, IEmployeeService employeeService, IShiftService shiftService, IDepartmentService departmentService, IEmployeeDailyAttendanceComputationService dailyAttendanceComputationService)
        {
            EmployeeAttendanceService = employeeAttendanceService;
            EmployeeService = employeeService;
            ShiftService = shiftService;
            DepartmentService = departmentService;
            DailyAttendanceComputationService = dailyAttendanceComputationService;
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
        public async Task<IActionResult> EmployeeAttendanceFilterAsync(int projectId, DateTime attendanceDate,
            CancellationToken cancellationToken)
        {
            var attendance = await EmployeeAttendanceService.GetDailyAttendanceAsync(projectId, attendanceDate,
                cancellationToken);
            return Ok(attendance);
        }

        [HttpGet("SavedAttendance")]
        public async Task<IActionResult> GetSavedAttendanceAsync(string? attendanceDate, int projectId = 0,
            int draw = 0, int start = 0, int length = 25, CancellationToken cancellationToken = default)
        {
            if (!DateOnly.TryParseExact(attendanceDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ||
                date == default || projectId < 0 || draw < 0 || start < 0 || length is not (10 or 25 or 50 or 100))
                return BadRequest(new { Message = "Enter a valid attendance date, project, and page." });

            return Ok(await DailyAttendanceComputationService.GetPageAsync(date, projectId, draw, start, length, cancellationToken));
        }

        [HttpPost("ReimportSavedAttendance")]
        public async Task<IActionResult> ReimportSavedAttendanceAsync(ReimportSavedAttendanceRequestModel request, CancellationToken cancellationToken)
        {
            if (request.ProjectId <= 0 || request.AttendanceDate is not { } date || date == default ||
                date > DateOnly.FromDateTime(PhilippineTime.Now))
                return BadRequest(new { Message = "Select a project and a current or past attendance date." });

            var actor = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Sid) ?? "Authenticated user";
            var audit = new ImportAudit(actor, "Saved Attendance Reimport", "SCIC HR Portal");
            var result = await DailyAttendanceComputationService.ReimportAsync(request.ProjectId, date, audit, cancellationToken);
            if (result is null)
                return NotFound(new { Message = "The selected project was not found or is inactive." });

            return Ok(result);
        }

        [HttpPost("Compute")]
        public async Task<IActionResult> ComputeAsync(EmployeeAttendanceUpdateRequestModel request,
            CancellationToken cancellationToken)
        {
            if (request.AttendanceDate is not { } date || date == default || request.ProjectId < 0 ||
                request.Edits is not { Count: > 0 } ||
                request.Edits.Any(edit => edit is null || edit.TimeLogId <= 0 || edit.ProjectId <= 0 ||
                    !edit.HasChanges || request.ProjectId != 0 && edit.ProjectId != request.ProjectId))
            {
                return BadRequest(new { Message = "Provide approval edits for the selected project and attendance date." });
            }

            var edits = request.Edits.OfType<AttendanceApprovalEdit>().ToArray();
            if (edits.DistinctBy(edit => (edit.ProjectId, edit.TimeLogId)).Count() != edits.Length)
                return BadRequest(new { Message = "Send each attendance row only once." });

            var actor = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Sid) ?? "Authenticated user";
            var audit = new ImportAudit(actor, "Attendance Compute", "SCIC HR Portal");
            var saved = await DailyAttendanceComputationService.SaveApprovalEditsAsync(date, edits, audit, cancellationToken);
            if (!saved)
                return Conflict(new { Message = "One or more attendance rows are unavailable or could not be recalculated. Check the project and date and review the saved attendance before retrying." });

            return StatusCode(201);
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
    }
}
