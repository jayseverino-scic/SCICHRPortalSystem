using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.API.Extensions;
using SCICHRPortal.API.Imports;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Jira;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize(Roles = "Administrator")]
    [ApiController]
    [Route("api/Authenticated/[controller]")]
    public class LeaveController : ControllerBase
    {
        private ILeaveService LeaveService { get; }

        public LeaveController(ILeaveService leaveService)
        {
            LeaveService = leaveService;
        }

        private string Actor
        {
            get
            {
                var name = User.Identity?.Name;
                if (!string.IsNullOrWhiteSpace(name) && name.Length <= 120)
                    return name;

                // SCIC permits display names longer than the audit columns. Preserve
                // attribution using the issued internal user ID rather than truncating a name.
                var userId = User.FindFirstValue(ClaimTypes.Sid);
                return !string.IsNullOrWhiteSpace(userId) && userId.Length <= 120 ? userId : "Authenticated user";
            }
        }

        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] LeavePageQuery query, CancellationToken cancellationToken)
        {
            if (!query.IsValid())
                return BadRequest(new { Message = "Enter valid paging, search, and sort options." });

            var page = await LeaveService.GetPageAsync(query, cancellationToken);
            return Ok(page);
        }

        [HttpGet("ApprovedRequests")]
        [Authorize(Policy = JiraRequestsExtensions.PolicyName)]
        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        public async Task<IActionResult> ApprovedRequestsAsync(
            [FromQuery] string? cursor, [FromServices] JiraRequestsPageReader jiraRequests, CancellationToken cancellationToken)
        {
            var result = await jiraRequests.GetPageAsync(JiraRequestType.Leave, cursor, cancellationToken);
            return result;
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> DetailsAsync(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
                return BadRequest(new { Message = "Select a valid record." });

            var record = await LeaveService.GetDetailsAsync(id, cancellationToken);
            return record == null ? NotFound(new { Message = "Leave record was not found." }) : Ok(record);
        }

        [HttpGet("EmployeeLookup")]
        public async Task<IActionResult> EmployeeLookupAsync(string? term, int page = 1, CancellationToken cancellationToken = default)
        {
            if (page < 1 || page > 100000 || (term?.Length ?? 0) > 100)
                return BadRequest(new { Message = "Enter a valid employee search and page." });

            var search = term?.Trim() ?? "";
            if (search.Length < 2)
                return Ok(new LeaveEmployeeLookupPage());

            var employees = await LeaveService.SearchEmployeesAsync(search, page, cancellationToken);
            return Ok(employees);
        }

        [HttpGet("LeaveTypes")]
        public async Task<IActionResult> LeaveTypesAsync(CancellationToken cancellationToken)
        {
            var leaveTypes = await LeaveService.GetLeaveTypesAsync(cancellationToken);
            return Ok(leaveTypes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAsync(LeaveWriteRequest request, CancellationToken cancellationToken)
        {
            var result = await LeaveService.CreateAsync(request, Actor, cancellationToken);
            return WriteResponse(result, created: true);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateAsync(int id, LeaveWriteRequest request, CancellationToken cancellationToken)
        {
            var result = await LeaveService.UpdateAsync(id, request, Actor, cancellationToken);
            return WriteResponse(result);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteAsync(int id, CancellationToken cancellationToken)
        {
            var result = await LeaveService.DeleteAsync(id, Actor, cancellationToken);
            return WriteResponse(result);
        }

        [HttpGet("ImportTemplate")]
        public async Task<IActionResult> ImportTemplateAsync(CancellationToken cancellationToken)
        {
            var types = await LeaveService.GetLeaveTypesAsync(cancellationToken);
            var workbook = await LeaveWorkbook.CreateTemplateAsync(types, cancellationToken);
            return File(workbook, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "LeaveImportTemplate.xlsx");
        }

        [HttpPost("Import")]
        [Consumes("multipart/form-data")]
        [RequestSizeLimit(LeaveWorkbook.MaximumFileBytes + 65536)]
        [RequestFormLimits(MultipartBodyLengthLimit = LeaveWorkbook.MaximumFileBytes + 65536)]
        public async Task<IActionResult> ImportAsync(IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0 || !string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Message = "Select a non-empty .xlsx workbook using the Leave template." });

            if (file.Length > LeaveWorkbook.MaximumFileBytes)
                return StatusCode(StatusCodes.Status413PayloadTooLarge, new { Message = "The workbook must be no larger than 10 MiB." });

            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            var parsed = LeaveWorkbook.Read(stream);
            if (parsed.Error != null)
                return BadRequest(new { Message = parsed.Error });

            var result = await LeaveService.ImportAsync(parsed.Rows, Actor, cancellationToken);
            return Ok(result);
        }

        private IActionResult WriteResponse(LeaveWriteResult result, bool created = false) => result.Status switch
        {
            LeaveWriteStatus.Invalid => BadRequest(new { result.Message }),
            LeaveWriteStatus.NotFound => NotFound(new { result.Message }),
            LeaveWriteStatus.Conflict => Conflict(new { result.Message }),
            _ => created ? StatusCode(201, result.Id) : Ok()
        };
    }
}
