using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Administration;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class EmployeeShiftController : ControllerBase
    {
        private IEmployeeShiftService EmployeeShiftService { get; }
        private IShiftService ShiftService { get; }
        public EmployeeShiftController(IEmployeeShiftService employeeShiftService, IShiftService shiftService)
        {
            EmployeeShiftService = employeeShiftService;
            ShiftService = shiftService;
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
        public async Task<IActionResult> EmployeeShiftFilterAsync(int projectId = 0, int shiftId = 0, string filterType = "All", int? skip = null, int? take = null, string? searchKeyword = null, CancellationToken cancellationToken = default)
        {
            if (projectId < 0 || shiftId < 0 || skip < 0 || take is < 1 or > 80 ||
                (searchKeyword?.Length ?? 0) > 200 || filterType is not ("All" or "Assigned" or "Unassigned"))
            {
                return BadRequest("Enter valid project, shift, status, paging and search filters.");
            }

            var result = await EmployeeShiftService.GetShiftFilterAsync(projectId, shiftId, filterType, skip, take, searchKeyword, cancellationToken);

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
        [HttpPost()]
        public async Task<IActionResult> UpdateShiftAssignmentAsync(List<EmployeeShiftUpdateRequestModel> employeeShift, int shiftId)
        {
            if (shiftId < 0 || (shiftId == 0 && employeeShift.Any(item => item.IsAssigned == true && !item.PreserveSchedule)))
                return BadRequest("Select a schedule to assign.");
            Shift? shift = null;
            if (employeeShift.Any(item => item.IsAssigned == true && !item.PreserveSchedule))
            {
                shift = await ShiftService.GetAsync(shiftId);
                if (shift == null) return BadRequest("The selected schedule is unavailable.");
            }
            DateTime? mondayShiftStart = shift?.MondayShiftStart;
            DateTime? mondayShiftEnd = shift?.MondayShiftEnd;
            DateTime? tuesdayShiftStart = shift?.TuesdayShiftStart;
            DateTime? tuesdayShiftEnd = shift?.TuesdayShiftEnd;
            DateTime? wednesdayShiftStart = shift?.WednesdayShiftStart;
            DateTime? wednesdayShiftEnd = shift?.WednesdayShiftEnd;
            DateTime? thursdayShiftStart = shift?.ThursdayShiftStart;
            DateTime? thursdayShiftEnd = shift?.ThursdayShiftEnd;
            DateTime? fridayShiftStart = shift?.FridayShiftStart;
            DateTime? fridayShiftEnd = shift?.FridayShiftEnd;
            DateTime? saturdayShiftStart = shift?.SaturdayShiftStart;
            DateTime? saturdayShiftEnd = shift?.SaturdayShiftEnd;
            DateTime? sundayShiftStart = shift?.SundayShiftStart;
            DateTime? sundayShiftEnd = shift?.SundayShiftEnd;

            foreach (var item in employeeShift)
            {
                if (item.PreserveSchedule)
                {
                    var updated = await EmployeeShiftService.UpdateFlagsAsync(item.AssignedShiftId, new EmployeeShiftAssignmentChange
                    {
                        EmployeeId = item.EmployeeId, IsFlexibleShift = item.IsFlexibleShift,
                        IsNoShift = item.IsNoShift, IsNoBreak = item.IsNoBreak
                    }, Actor);
                    if (!updated) return NotFound("The employee assignment is unavailable. Your pending changes have been kept.");
                    continue;
                }
                if (item.IsNoShift) item.IsFlexibleShift = false;
                if (item.IsAssigned == true && item.AssignedShiftId != 0)
                {
                    EmployeeShift shiftAssignment = new EmployeeShift
                    {
                        AssignedShiftId = item.AssignedShiftId,
                        ShiftId = shiftId,
                        EmployeeId = item.EmployeeId,
                        DepartmentId = item.DepartmentId == 0 ? null : item.DepartmentId,
                        ProjectId = item.ProjectId == 0 ? null : item.ProjectId,
                        ShiftDate = DateTime.Now,
                        MondayShiftStart = mondayShiftStart,
                        MondayShiftEnd = mondayShiftEnd,
                        TuesdayShiftStart = tuesdayShiftStart,
                        TuesdayShiftEnd = tuesdayShiftEnd,
                        WednesdayShiftStart = wednesdayShiftStart,
                        WednesdayShiftEnd = wednesdayShiftEnd,
                        ThursdayShiftStart = thursdayShiftStart,
                        ThursdayShiftEnd = thursdayShiftEnd,
                        FridayShiftStart = fridayShiftStart,
                        FridayShiftEnd = fridayShiftEnd,
                        SaturdayShiftStart = saturdayShiftStart,
                        SaturdayShiftEnd = saturdayShiftEnd,
                        SundayShiftStart = sundayShiftStart,
                        SundayShiftEnd = sundayShiftEnd,
                        IsFlexibleShift = item.IsFlexibleShift,
                        IsNoBreak = item.IsNoBreak,
                        IsNoShift = item.IsNoShift,
                        UpdatedBy = Actor
                    };
                    await EmployeeShiftService.UpdateAsync(shiftAssignment);
                }
                if (item.IsAssigned == true && item.AssignedShiftId == 0)
                {
                    EmployeeShift shiftAssignment = new EmployeeShift
                    {
                        AssignedShiftId = item.AssignedShiftId,
                        ShiftId = shiftId,
                        EmployeeId = item.EmployeeId,
                        DepartmentId = item.DepartmentId == 0 ? null : item.DepartmentId,
                        ProjectId = item.ProjectId == 0 ? null : item.ProjectId,
                        ShiftDate = DateTime.Now,
                        MondayShiftStart = mondayShiftStart,
                        MondayShiftEnd = mondayShiftEnd,
                        TuesdayShiftStart = tuesdayShiftStart,
                        TuesdayShiftEnd = tuesdayShiftEnd,
                        WednesdayShiftStart = wednesdayShiftStart,
                        WednesdayShiftEnd = wednesdayShiftEnd,
                        ThursdayShiftStart = thursdayShiftStart,
                        ThursdayShiftEnd = thursdayShiftEnd,
                        FridayShiftStart = fridayShiftStart,
                        FridayShiftEnd = fridayShiftEnd,
                        SaturdayShiftStart = saturdayShiftStart,
                        SaturdayShiftEnd = saturdayShiftEnd,
                        SundayShiftStart = sundayShiftStart,
                        SundayShiftEnd = sundayShiftEnd,
                        IsFlexibleShift = item.IsFlexibleShift,
                        IsNoBreak = item.IsNoBreak,
                        IsNoShift = item.IsNoShift,
                        CreatedBy = Actor,
                        CreatedAt = DateTime.UtcNow
                    };
                    try
                    {
                        await EmployeeShiftService.InsertAsync(shiftAssignment);
                    }
                    catch (EmployeeShiftAssignmentConflictException)
                    {
                        return Conflict("An employee was assigned while saving. Refresh the affected employee and try again.");
                    }
                }
                if (item.IsAssigned == false && item.AssignedShiftId != 0)
                {
                    await EmployeeShiftService.DeleteAsync(item.AssignedShiftId);
                }
            }

            return StatusCode(201, employeeShift);
        }

        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(EmployeeShift employeeShift)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            employeeShift.UpdatedAt = DateTime.UtcNow;
            employeeShift.UpdatedBy = "manuel";
            await EmployeeShiftService.UpdateAsync(employeeShift);

            return Ok();
        }

        [Authorize]
        [HttpDelete("{employeeShiftId}")]
        public async Task<IActionResult> RemoveAsync(int employeeShiftId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await EmployeeShiftService.DeleteAsync(employeeShiftId);

            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
