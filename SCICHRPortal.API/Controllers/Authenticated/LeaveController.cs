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
    public partial class LeaveController : ControllerBase
    {
        private ILeaveService LeaveService { get; }
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

        public LeaveController(ILeaveService leaveService)
        {
            LeaveService = leaveService;
        }

        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] LeavePageQuery query, CancellationToken cancellationToken)
        {
            if (!query.IsValid()) return BadRequest(new { Message = "Enter valid paging, search, and sort options." });
            return Ok(await LeaveService.GetPageAsync(query, cancellationToken));
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> DetailsAsync(int id, CancellationToken cancellationToken)
        {
            if (id <= 0) return BadRequest(new { Message = "Select a valid record." });
            var record = await LeaveService.GetDetailsAsync(id, cancellationToken);
            return record == null ? NotFound(new { Message = "Leave record was not found." }) : Ok(record);
        }

        [HttpGet("EmployeeLookup")]
        public async Task<IActionResult> EmployeeLookupAsync(string? term, int page = 1, CancellationToken cancellationToken = default)
        {
            if (page < 1 || page > 100000 || (term?.Length ?? 0) > 100)
                return BadRequest(new { Message = "Enter a valid employee search and page." });
            var search = term?.Trim() ?? "";
            return Ok(search.Length < 2 ? new LeaveEmployeeLookupPage()
                : await LeaveService.SearchEmployeesAsync(search, page, cancellationToken));
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(LeaveWriteRequest request, CancellationToken cancellationToken) =>
            WriteResponse(await LeaveService.CreateAsync(request, Actor, cancellationToken), created: true);

        [HttpGet("LeaveTypes")]
        public async Task<IActionResult> LeaveTypesAsync(CancellationToken cancellationToken) =>
            Ok(await LeaveService.GetLeaveTypesAsync(cancellationToken));

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, LeaveWriteRequest request, CancellationToken cancellationToken) =>
            WriteResponse(await LeaveService.UpdateAsync(id, request, Actor, cancellationToken));

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken) =>
            WriteResponse(await LeaveService.DeleteAsync(id, Actor, cancellationToken));

        private IActionResult WriteResponse(LeaveWriteResult result, bool created = false) => result.Status switch
        {
            LeaveWriteStatus.Invalid => BadRequest(new { result.Message }),
            LeaveWriteStatus.NotFound => NotFound(new { result.Message }),
            LeaveWriteStatus.Conflict => Conflict(new { result.Message }),
            _ => created ? StatusCode(201, result.Id) : Ok()
        };
    }
}
