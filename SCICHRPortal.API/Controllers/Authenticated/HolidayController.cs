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
    public class HolidayController : ControllerBase
    {
        private IHolidayService HolidayService { get; }
        public HolidayController(IHolidayService holidayService)
        {
            HolidayService = holidayService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var holiday = await HolidayService.GetAllAsync();
            return Ok(holiday);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await HolidayService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.HolidayId,
                d.HolidayName,
                d.HolidayType,
                d.HolidayDate,
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
        public async Task<IActionResult> InsertAsync(Holiday holiday)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await HolidayService.HasDuplicateName(holiday);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);

            await HolidayService.InsertAsync(holiday);

            return StatusCode(201, holiday.HolidayId);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Holiday holiday)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var updated = await HolidayService.UpdateAsync(holiday);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{holidayId}")]
        public async Task<IActionResult> DeleteAsync(int holidayId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await HolidayService.DeleteAsync(holidayId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
