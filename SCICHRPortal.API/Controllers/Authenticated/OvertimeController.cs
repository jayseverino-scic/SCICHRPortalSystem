using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("api/Authenticated/[controller]")]
    public sealed class OvertimeController(IOvertimeService overtimeService) : ControllerBase
    {
        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] OvertimePageQuery query, CancellationToken cancellationToken)
        {
            if (!query.IsValid())
            {
                return BadRequest(new { Message = "Enter valid paging and search options." });
            }

            var page = await overtimeService.GetPageAsync(query, cancellationToken);
            return page == null ? Forbid() : Ok(page);
        }
    }
}
