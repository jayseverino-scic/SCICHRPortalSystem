using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.API.Imports;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    public partial class LeaveController
    {
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
        public async Task<IActionResult> ImportAsync([FromForm] IFormFile? file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0 || !string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
                return BadRequest(new { Message = "Select a non-empty .xlsx workbook using the Leave template." });
            if (file.Length > LeaveWorkbook.MaximumFileBytes)
                return StatusCode(StatusCodes.Status413PayloadTooLarge, new { Message = "The workbook must be no larger than 10 MiB." });
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream, cancellationToken);
            var parsed = LeaveWorkbook.Read(stream);
            if (parsed.Error != null) return BadRequest(new { Message = parsed.Error });
            var result = await LeaveService.ImportAsync(parsed.Rows, Actor, cancellationToken);
            return Ok(result);
        }
    }
}
