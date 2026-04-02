using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class CutOffController : ControllerBase
    {
        private ICutOffService CutOffService { get; }
        public CutOffController(ICutOffService cutOffService)
        {
            CutOffService = cutOffService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var cutOffs = await CutOffService.GetAllAsync();
            return Ok(cutOffs);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await CutOffService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.CutOffId,
                d.StartDate,
                d.EndDate,
                d.IsActive,
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
        public async Task<IActionResult> InsertAsync(CutOff cutOff)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await CutOffService.HasDuplicateName(cutOff);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);

            await CutOffService.InsertAsync(cutOff);

            return StatusCode(201, cutOff.CutOffId);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(CutOff cutOff)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var updated = await CutOffService.UpdateAsync(cutOff);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{cutOffId}")]
        public async Task<IActionResult> DeleteAsync(int cutOffId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await CutOffService.DeleteAsync(cutOffId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
