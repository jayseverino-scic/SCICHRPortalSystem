using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [ApiController]
    [Route("api/Authenticated/[controller]")]
    public class TardinessController : ControllerBase
    {
        private ITardinessService TardinessService { get; }

        public TardinessController(ITardinessService tardinessService) => TardinessService = tardinessService;

        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] TardinessPageQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid || !query.IsValid()) return BadRequest("Enter valid From/To dates, project, search, sorting and paging.");
            return Ok(await TardinessService.GetPageAsync(query, cancellationToken));
        }

        [HttpGet("Export")]
        public async Task<IActionResult> ExportAsync([FromQuery] TardinessFilterQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid || !query.IsValid()) return BadRequest("Enter valid From/To dates, project, search and sorting.");
            return Ok(await TardinessService.GetExportAsync(query, cancellationToken));
        }
    }
}
