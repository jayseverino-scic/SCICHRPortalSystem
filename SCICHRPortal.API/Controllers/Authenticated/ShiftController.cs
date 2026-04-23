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
    public class ShiftController : ControllerBase
    {
        private IShiftService ShiftService { get; }
        public ShiftController(IShiftService shiftService)
        {
            ShiftService = shiftService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var shift = await ShiftService.GetAllAsync();
            return Ok(shift);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await ShiftService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.ShiftId,
                d.ShiftName,
                d.ShiftStart,
                d.ShiftEnd,
                d.BreakStart,
                d.BreakEnd,
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
        public async Task<IActionResult> InsertAsync(Shift shift)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await ShiftService.HasDuplicateName(shift);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            shift.CreatedAt = DateTime.UtcNow;
            shift.CreatedBy = "manuel";
            await ShiftService.InsertAsync(shift);

            return StatusCode(201, shift.ShiftId);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Shift shift)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            shift.UpdatedBy = "manuel";
            shift.UpdatedAt = DateTime.UtcNow;
            var updated = await ShiftService.UpdateAsync(shift);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{shiftId}")]
        public async Task<IActionResult> DeleteAsync(int shiftId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await ShiftService.DeleteAsync(shiftId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
