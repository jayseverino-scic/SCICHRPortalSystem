using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Helpers;
using System.Security.Claims;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class EmployeeShiftController : ControllerBase
    {
        private IEmployeeShiftService EmployeeShiftService { get; }
        public EmployeeShiftController(IEmployeeShiftService employeeShiftService)
        {
            EmployeeShiftService = employeeShiftService;
        }

        [Authorize] 
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var employeeShifts = await EmployeeShiftService.GetAllAsync();
            return Ok(employeeShifts);
        }

        private string Actor => User?.Identity?.Name ?? User?.FindFirstValue(ClaimTypes.Sid) ?? "Authenticated user";

        [Authorize]
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber = 1, int pageSize = 10, string? searchKeyword = null, CancellationToken cancellationToken = default)
        {
            if (pageNumber < 1 || pageSize is < 1 or > 80 ||
                (long)pageNumber * pageSize > int.MaxValue || (searchKeyword?.Length ?? 0) > 200)
            {
                return BadRequest("Enter valid paging and search filters.");
            }

            var skip = (pageNumber - 1) * pageSize;
            var result = await EmployeeShiftService.GetShiftFilterAsync(0, 0, "All", skip, pageSize, searchKeyword, cancellationToken);
            // Preserve the legacy Filter row names while sharing the database-paged query.
            var data = result.Data.Select((d, index) => new
            {
                d.AssignedShiftId,
                d.ShiftDate,
                d.MondayShiftStart,
                d.MondayShiftEnd,
                d.TuesdayShiftStart,
                d.TuesdayShiftEnd,
                d.WednesdayShiftStart,
                d.WednesdayShiftEnd,
                d.ThursdayShiftStart,
                d.ThursdayShiftEnd,
                d.FridayShiftStart,
                d.FridayShiftEnd,
                d.SaturdayShiftStart,
                d.SaturdayShiftEnd,
                d.SundayShiftStart,
                d.SundayShiftEnd,
                d.IsFlexibleShift,
                d.IsNoBreak,
                d.IsNoShift,
                d.EmployeeId,
                d.EmployeeName,
                d.DepartmentId,
                d.DepartmentName,
                d.ProjectId,
                CompanyBranchName = d.ProjectName,
                d.ShiftId,
                d.ShiftName,
                OrderNumber = skip + index + 1
            }).ToList();
            var dto = new
            {
                Data = data,
                Total = result.FilteredTotal
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpGet("ShiftFilter")]
        public async Task<IActionResult> EmployeeShiftFilterAsync(int projectId = 0, int shiftId = 0, string filterType = "All", int? skip = null, int? take = null, string? searchKeyword = null, CancellationToken cancellationToken = default, DateTime? asOf = null)
        {
            if (!PhilippineTime.IsLocal(asOf) || projectId < 0 || shiftId < 0 || skip < 0 || take is < 1 or > 80 ||
                (searchKeyword?.Length ?? 0) > 200 || filterType is not ("All" or "Assigned" or "Unassigned"))
            {
                return BadRequest("Enter valid project, shift, status, paging and search filters.");
            }

            var result = await EmployeeShiftService.GetShiftFilterAsync(projectId, shiftId, filterType, skip, take, searchKeyword, cancellationToken, asOf);

            if (!skip.HasValue && !take.HasValue)
            {
                return Ok(result.Data);
            }

            return Ok(result);
        }
        [Authorize]
        [HttpPost("AssignFiltered")]
        public async Task<IActionResult> AssignFilteredAsync(EmployeeShiftFilteredAssignmentRequest request, CancellationToken cancellationToken = default)
        {
            var result = await EmployeeShiftService.AssignFilteredAsync(request, Actor, cancellationToken);
            return result.Status switch
            {
                EmployeeShiftAssignmentStatus.Success => Ok(new { result.Affected }),
                EmployeeShiftAssignmentStatus.Conflict => Conflict(result.Message),
                _ => BadRequest(result.Message)
            };
        }

        [Authorize]
        [HttpGet("History/{employeeId:int}")]
        public async Task<IActionResult> HistoryAsync(int employeeId, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            if (employeeId <= 0 || skip < 0 || take is < 1 or > 80) return BadRequest("Enter valid employee and paging values.");
            return Ok(await EmployeeShiftService.GetHistoryAsync(employeeId, skip, take, cancellationToken));
        }

        [Authorize]
        [HttpPost()]
        public Task<IActionResult> UpdateShiftAssignmentAsync(List<EmployeeShiftUpdateRequestModel> employeeShift, int shiftId,
            DateTime? effectiveStartDate = null, DateTime? effectiveEndDate = null, bool isTemporary = false, DateTime? asOf = null,
            CancellationToken cancellationToken = default)
        {
            var request = new EmployeeShiftFilteredAssignmentRequest
            {
                EffectiveStartDate = effectiveStartDate ?? PhilippineTime.Now, EffectiveEndDate = effectiveEndDate,
                IsTemporary = isTemporary, AsOf = asOf, ApplyAssignmentToFilter = false, ScheduleId = shiftId,
                Changes = employeeShift.Select(row => new EmployeeShiftAssignmentChange
                {
                    EmployeeId = row.EmployeeId, PreserveSchedule = row.PreserveSchedule, IsAssigned = row.IsAssigned == true,
                    IsFlexibleShift = row.IsFlexibleShift, IsNoShift = row.IsNoShift, IsNoBreak = row.IsNoBreak
                }).ToList()
            };
            return AssignFilteredAsync(request, cancellationToken);
        }

        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(EmployeeShift employeeShift)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            if (employeeShift.AssignedShiftId <= 0 || employeeShift.EmployeeId <= 0 || employeeShift.ShiftId <= 0 ||
                !PhilippineTime.IsLocal(employeeShift.EffectiveStartDate) || !PhilippineTime.IsLocal(employeeShift.EffectiveEndDate) ||
                (employeeShift.IsTemporary && (!employeeShift.EffectiveStartDate.HasValue || !employeeShift.EffectiveEndDate.HasValue ||
                    employeeShift.EffectiveEndDate <= employeeShift.EffectiveStartDate)) || (!employeeShift.IsTemporary && employeeShift.EffectiveEndDate.HasValue))
                return BadRequest("Enter a valid assignment and local effective period.");
            employeeShift.UpdatedAt = DateTime.UtcNow;
            employeeShift.UpdatedBy = Actor;
            try
            {
                if (!await EmployeeShiftService.UpdateAsync(employeeShift)) return NotFound(ResponseMessage.NotFound);
                return Ok();
            }
            catch (EmployeeShiftAssignmentConflictException)
            {
                return Conflict("The assignment timeline changed. Refresh and choose a distinct effective start.");
            }
        }

        [Authorize]
        [HttpDelete("{employeeShiftId}")]
        public async Task<IActionResult> RemoveAsync(int employeeShiftId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await EmployeeShiftService.DeleteAsync(employeeShiftId, Actor);

            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
