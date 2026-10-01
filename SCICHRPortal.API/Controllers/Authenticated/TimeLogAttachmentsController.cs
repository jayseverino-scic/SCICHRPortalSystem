using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using SCICHRPortal.API.Storage;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using System.Security.Claims;

namespace SCICHRPortal.API.Controllers.Authenticated;

[Authorize]
[ApiController]
[Route("api/Authenticated/EmployeeTimeLog/{id:int}/attachment")]
public class TimeLogAttachmentsController(
    IEmployeeTimeLogService records,
    ITimeLogAttachmentStorage storage,
    ILogger<TimeLogAttachmentsController> logger) : ControllerBase
{
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(11 * 1024 * 1024)]
    [RequestFormLimits(MultipartBodyLengthLimit = 11 * 1024 * 1024)]
    public async Task<IActionResult> UploadAsync(int id, [FromForm] List<IFormFile> file, [FromForm] long? version)
    {
        if (!ModelState.IsValid || file is not { Count: 1 } || version is null || version < 0)
            return BadRequest("Select exactly one attachment and reload the current record before uploading.");
        var upload = file[0];
        if (upload.Length <= 0 || upload.Length > TimeLogAttachmentValidation.MaxBytes)
            return BadRequest("The attachment must be nonempty and no larger than 10 MB.");
        var record = await records.GetAsync(id);
        if (record is null || record.Deleted) return NotFound();
        if (record.Attachment is not null) return Conflict("This time record already has an attachment.");
        if (version != record.Version) return Conflict("This time record changed. Close and reopen it before uploading.");
        var filename = Path.GetFileName(upload.FileName.Replace('\\', '/'));
        if (string.IsNullOrWhiteSpace(filename) || filename.Length > 255 || filename.Any(char.IsControl))
            return BadRequest("Use a valid filename of up to 255 characters.");
        await using var input = upload.OpenReadStream();
        var contentType = await TimeLogAttachmentValidation.ContentTypeAsync(filename, input);
        if (contentType is null) return BadRequest("Allowed attachments: PDF, JPG/JPEG, PNG, DOCX, XLS and XLSX. The contents must match the file type.");
        input.Position = 0;
        string? key = null;
        bool committed = false;
        try
        {
            key = await storage.SaveAsync(input, HttpContext.RequestAborted);
            var actor = User.Identity?.Name ?? User.FindFirstValue(ClaimTypes.Sid) ?? "Authenticated user";
            var now = DateTime.UtcNow;
            record.Attachment = new TimeLogAttachment
            {
                TimeLogId = id, Provider = storage.Provider, StorageKey = key, FileName = filename,
                ContentType = contentType, Size = upload.Length, UploadedBy = actor, UploadedAt = now
            };
            record.Comment = TimeLogChanges.Append(record.Comment, actor, [$"Attachment added: {filename}"], now);
            record.UpdatedBy = actor;
            record.UpdatedAt = now;
            record.Version++;
            if (!await records.UpdateAsync(record)) return NotFound();
            committed = true;
            return Ok(new { record.Version, record.Comment, Attachment = new {
                record.Attachment.TimeLogAttachmentId, record.Attachment.FileName, record.Attachment.Size
            } });
        }
        catch (DbUpdateConcurrencyException)
        {
            return Conflict("This time record changed. Close and reopen it before uploading.");
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            return Conflict("This time record already has an attachment.");
        }
        catch (Exception ex) when (ex is IOException or DbUpdateException or UnauthorizedAccessException)
        {
            logger.LogError(ex, "Unable to save attachment for time record {TimeLogId}", id);
            return StatusCode(500, "The attachment could not be saved. Your time record is saved; retry the attachment.");
        }
        finally
        {
            if (!committed && key is not null)
            {
                try { await storage.DeleteAsync(key); }
                catch (Exception ex) { logger.LogError(ex, "Unable to clean up attachment {StorageKey}", key); }
            }
        }
    }

    [HttpGet("{attachmentId:int}")]
    public async Task<IActionResult> DownloadAsync(int id, int attachmentId)
    {
        var record = await records.GetAsync(id);
        if (record is null || record.Deleted || record.Attachment is not { } attachment ||
            attachment.TimeLogAttachmentId != attachmentId) return NotFound();
        if (attachment.Provider != storage.Provider) return StatusCode(503, "Attachment storage is unavailable.");
        try
        {
            var stream = await storage.OpenAsync(attachment.StorageKey, HttpContext.RequestAborted);
            Response.Headers["X-Content-Type-Options"] = "nosniff";
            return File(stream, attachment.ContentType, attachment.FileName);
        }
        catch (FileNotFoundException) { return NotFound("The stored attachment is unavailable."); }
        catch (DirectoryNotFoundException) { return NotFound("The stored attachment is unavailable."); }
    }
}
