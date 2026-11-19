using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("api/Authenticated/[controller]")]
    public partial class OfficialBusinessController : ControllerBase
    {
        private IOfficialBusinessService OfficialBusinessService { get; }
        private string Actor
        {
            get
            {
                var name = User.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(name) && name.Length <= 120) return name;
                // SCIC permits display names longer than the audit columns. Preserve
                // attribution using the issued internal user ID rather than truncating a name.
                var userId = User.FindFirstValue(ClaimTypes.Sid);
                return !string.IsNullOrWhiteSpace(userId) && userId.Length <= 120 ? userId : "Authenticated user";
            }
        }

        public OfficialBusinessController(IOfficialBusinessService officialBusinessService)
        {
            OfficialBusinessService = officialBusinessService;
        }

        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] OfficialBusinessPageQuery query, CancellationToken cancellationToken)
        {
            if (!query.IsValid()) return BadRequest(new { Message = "Enter valid paging, search, and sort options." });
            return Ok(await OfficialBusinessService.GetPageAsync(query, cancellationToken));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> DetailsAsync(int id, CancellationToken cancellationToken)
        {
            if (id <= 0) return BadRequest(new { Message = "Select a valid record." });
            var record = await OfficialBusinessService.GetDetailsAsync(id, cancellationToken);
            return record == null ? NotFound(new { Message = "Official Business record was not found." }) : Ok(record);
        }

        [HttpGet("EmployeeLookup")]
        public async Task<IActionResult> EmployeeLookupAsync(string? term, int page = 1, CancellationToken cancellationToken = default)
        {
            if (page < 1 || page > 100000 || (term?.Length ?? 0) > 100)
                return BadRequest(new { Message = "Enter a valid employee search and page." });
            var search = term?.Trim() ?? "";
            return Ok(search.Length < 2 ? new OfficialBusinessEmployeeLookupPage()
                : await OfficialBusinessService.SearchEmployeesAsync(search, page, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(OfficialBusinessWriteRequest request, CancellationToken cancellationToken) =>
            WriteResponse(await OfficialBusinessService.CreateAsync(request, Actor, cancellationToken), created: true);

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, OfficialBusinessWriteRequest request, CancellationToken cancellationToken) =>
            WriteResponse(await OfficialBusinessService.UpdateAsync(id, request, Actor, cancellationToken));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
            WriteResponse(await OfficialBusinessService.DeleteAsync(id, Actor, cancellationToken));

        private IActionResult WriteResponse(OfficialBusinessWriteResult result, bool created = false) => result.Status switch
        {
            OfficialBusinessWriteStatus.Invalid => BadRequest(new { result.Message }),
            OfficialBusinessWriteStatus.NotFound => NotFound(new { result.Message }),
            OfficialBusinessWriteStatus.Conflict => Conflict(new { result.Message }),
            _ => created ? StatusCode(201, result.Id) : Ok()
        };
    }
}
