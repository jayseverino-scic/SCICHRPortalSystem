using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Drawing.Printing;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class LeaveTypeController : ControllerBase
    {
        private ILeaveTypeService LeaveTypeService { get; }
        public LeaveTypeController(ILeaveTypeService leaveTypeService)
        {
            LeaveTypeService = leaveTypeService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var leaveType = await LeaveTypeService.GetAllAsync();
            return Ok(leaveType);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await LeaveTypeService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.LeaveTypeId,
                d.LeaveDescription,
                d.AllowedDays,
                d.IsPaid,
                IsTodayAnnouncement = dateToday.Date == d.CreatedAt.Date,
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

        [HttpPost()]
        public async Task<IActionResult> InsertAsync(LeaveType leaveType)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await LeaveTypeService.HasDuplicateName(leaveType);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            leaveType.CreatedAt = DateTime.Now;
            leaveType.CreatedBy = "manuel";
            await LeaveTypeService.InsertAsync(leaveType);

            return StatusCode(201, leaveType.LeaveTypeId);
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(LeaveType leaveType)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            leaveType.UpdatedAt = DateTime.Now;
            leaveType.UpdatedBy = "manuel";
            var updated = await LeaveTypeService.UpdateAsync(leaveType);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        [HttpDelete("{leaveTypeId}")]
        public async Task<IActionResult> DeleteAsync(int leaveTypeId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await LeaveTypeService.DeleteAsync(leaveTypeId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

    }
}
