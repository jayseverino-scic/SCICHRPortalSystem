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
    public class PositionController : ControllerBase
    {
        private IPositionService PositionService { get; }
        public PositionController(IPositionService positionService)
        {
            PositionService = positionService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var position = await PositionService.GetAllAsync();
            return Ok(position);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await PositionService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.PositionId,
                d.PositionName,
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
        public async Task<IActionResult> InsertAsync(Position position)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await PositionService.HasDuplicateName(position);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            position.CreatedBy = "manuel";
            position.CreatedAt = DateTime.Now;
            await PositionService.InsertAsync(position);

            return StatusCode(201, position.PositionId);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Position position)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            position.UpdatedAt = DateTime.Now;
            position.UpdatedBy = "manuel";
            var updated = await PositionService.UpdateAsync(position);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{positionId}")]
        public async Task<IActionResult> DeleteAsync(int positionId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await PositionService.DeleteAsync(positionId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
