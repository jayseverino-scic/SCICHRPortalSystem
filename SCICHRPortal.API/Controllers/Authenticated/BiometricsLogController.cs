using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using System.Globalization;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class BiometricsLogController : ControllerBase
    {
        private readonly IBiometricsLogService _biometricsLogService;
        private readonly IAttendanceImportService _attendanceImport;

        public BiometricsLogController(IBiometricsLogService biometricsLogService, IAttendanceImportService attendanceImport)
        {
            _biometricsLogService = biometricsLogService;
            _attendanceImport = attendanceImport;
        }

        // ============ EXISTING GET METHODS ============

        [HttpGet("Page")]
        public async Task<IActionResult> PageAsync([FromQuery] BiometricsLogPageQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid || !query.IsValid())
                return BadRequest("Enter valid paging, search, sort and date filters.");
            return Ok(await _biometricsLogService.GetPageAsync(query, cancellationToken));
        }

        [HttpGet("Export")]
        public async Task<IActionResult> ExportAsync([FromQuery] BiometricsLogFilterQuery query, CancellationToken cancellationToken = default)
        {
            if (!ModelState.IsValid || !query.IsValid())
                return BadRequest("Enter valid search, sort and date filters.");
            return Ok(await _biometricsLogService.GetExportAsync(query, cancellationToken));
        }

        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var biometricsLogs = await _biometricsLogService.GetAllAsync();
            return Ok(biometricsLogs);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            var tuple = await _biometricsLogService.FilterAsync(pageNumber, pageSize, searchKeyword!, startDate, endDate, deviceName);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.BiometricsLogId,
                d.PersonnelId,
                d.LastName,
                d.FirstName,
                d.Date,
                d.Time,
                d.LogType,
                d.DeviceName,
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

        [HttpGet("FilterPerProject")]
        public async Task<IActionResult> FilterPerProjectAsync(DateTime? startDate, DateTime? endDate, string? projectName)
        {
            var tuple = await _biometricsLogService.FilterByProjectAndDateRange(startDate, endDate, projectName);

            var data = tuple.Select(d => new
            {
                d.BiometricsLogId,
                d.PersonnelId,
                d.LastName,
                d.FirstName,
                d.Date,
                d.Time,
                d.LogType,
                d.DeviceName,
                d.CreatedAt,
            });

            var dto = new
            {
                Data = data,
                Total = data.Count()
            };
            return Ok(dto);
        }

        // ============ EXISTING POST/PUT METHODS ============

        [HttpPost()]
        public async Task<IActionResult> InsertAsync(BiometricsLog biometricsLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            await _biometricsLogService.InsertAsync(biometricsLog);
            return StatusCode(201, biometricsLog.BiometricsLogId);
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(BiometricsLog biometricsLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var updated = await _biometricsLogService.UpdateAsync(biometricsLog);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        // ============ OPTIMIZED BULK IMPORT METHODS ============

        /// <summary>
        /// Ultra-fast Excel import using SqlBulkCopy
        /// </summary>
        [HttpPost("Import")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult> UploadFileAsync(IFormFile file, [FromForm] string? projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return BadRequest("Select a project before importing an Excel file.");
            if (file == null)
                return BadRequest(ResponseMessage.BadRequest);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xls" && extension != ".xlsx")
            {
                return StatusCode(415, ResponseMessage.FileNotSupported);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var biometricsLogs = new List<BiometricsLog>();
            var errors = new List<string>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                var rowCount = workSheet.Dimension.Rows;

                // Process all rows
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var personnelId = workSheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                        var lastName = workSheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
                        var firstName = workSheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                        var date = workSheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";
                        var time = workSheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                        var logType = workSheet.Cells[row, 6].Value?.ToString()?.Trim() ?? "";
                        var deviceName = workSheet.Cells[row, 7].Value?.ToString()?.Trim() ?? "";

                        if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
                        {
                            errors.Add($"Date or time missing at row {row}");
                            continue;
                        }

                        DateTime parsedDate = DateTime.ParseExact(
                            date,
                            "MM-dd-yyyy",
                            CultureInfo.InvariantCulture
                        );

                        DateTime parsedTime = DateTime.ParseExact(
                            time,
                            "HH:mm:ss tt",
                            CultureInfo.InvariantCulture
                        );

                        biometricsLogs.Add(new BiometricsLog
                        {
                            PersonnelId = personnelId,
                            LastName = lastName,
                            FirstName = firstName,
                            Date = parsedDate,
                            Time = parsedDate.Date + parsedTime.TimeOfDay,
                            LogType = logType,
                            DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName,
                            ProjectName = projectName.Trim(),
                            ImportSource = "File",
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Manuel"
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Invalid data format at row {row}: {ex.Message}");
                    }
                }

                // Bulk insert using service
                if (biometricsLogs.Any())
                {
                    var result = await _biometricsLogService.BulkInsertWithResultAsync(biometricsLogs);

                    stopwatch.Stop();

                    return Ok(new
                    {
                        TotalProcessed = biometricsLogs.Count,
                        Inserted = result.Inserted,
                        Failed = result.Failed,
                        Errors = result.Errors,
                        ImportTimeMs = stopwatch.ElapsedMilliseconds,
                        RecordsPerSecond = biometricsLogs.Count / (stopwatch.ElapsedMilliseconds / 1000.0)
                    });
                }

                return Ok(new
                {
                    Message = "No valid records to import",
                    Total = 0,
                    Errors = errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Ultra-fast database import using SqlBulkCopy
        /// </summary>
        [HttpGet("ImportDb")]
        [Authorize]
        public async Task<ActionResult> ImportDb(DateTime? startImport, DateTime? endImport, string? projectName)
        {
            if (!startImport.HasValue || !endImport.HasValue || string.IsNullOrWhiteSpace(projectName))
                return BadRequest("Select a project and both dates before importing records.");
            if (startImport > endImport)
                return BadRequest("From date must be on or before To date.");

            try
            {
                var result = await _attendanceImport.ImportBiometricsAsync(startImport.Value, endImport.Value, projectName, "Manuel");
                if (result.TotalFetched == 0)
                    return Ok(new { Message = "No records found to import", Total = 0 });
                if (result.ProcessedCount == 0)
                    return Ok(new
                    {
                        result.TotalFetched,
                        result.ProcessedCount,
                        result.SkippedCount,
                        Message = "No valid records to import"
                    });

                return Ok(new
                {
                    result.TotalFetched,
                    result.ProcessedCount,
                    result.SkippedCount,
                    result.DuplicateCount,
                    result.Inserted,
                    result.Failed,
                    result.Errors,
                    result.ImportTimeMs,
                    RecordsPerSecond = result.ProcessedCount / (Math.Max(1, result.ImportTimeMs) / 1000.0)
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Import with progress tracking (for large files)
        /// </summary>
        [HttpPost("ImportWithProgress")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult> UploadFileWithProgressAsync(IFormFile file, [FromForm] string? projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
                return BadRequest("Select a project before importing an Excel file.");
            if (file == null)
                return BadRequest(ResponseMessage.BadRequest);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (extension != ".xls" && extension != ".xlsx")
            {
                return StatusCode(415, ResponseMessage.FileNotSupported);
            }

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var biometricsLogs = new List<BiometricsLog>();
            var errors = new List<string>();

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                var rowCount = workSheet.Dimension.Rows;

                // Process all rows
                for (int row = 2; row <= rowCount; row++)
                {
                    try
                    {
                        var personnelId = workSheet.Cells[row, 1].Value?.ToString()?.Trim() ?? "";
                        var lastName = workSheet.Cells[row, 2].Value?.ToString()?.Trim() ?? "";
                        var firstName = workSheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                        var date = workSheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";
                        var time = workSheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                        var logType = workSheet.Cells[row, 6].Value?.ToString()?.Trim() ?? "";
                        var deviceName = workSheet.Cells[row, 7].Value?.ToString()?.Trim() ?? "";

                        if (string.IsNullOrWhiteSpace(date) || string.IsNullOrWhiteSpace(time))
                        {
                            errors.Add($"Date or time missing at row {row}");
                            continue;
                        }

                        DateTime parsedDate = DateTime.ParseExact(
                            date,
                            "MM-dd-yyyy",
                            CultureInfo.InvariantCulture
                        );

                        DateTime parsedTime = DateTime.ParseExact(
                            time,
                            "HH:mm:ss tt",
                            CultureInfo.InvariantCulture
                        );

                        biometricsLogs.Add(new BiometricsLog
                        {
                            PersonnelId = personnelId,
                            LastName = lastName,
                            FirstName = firstName,
                            Date = parsedDate,
                            Time = parsedDate.Date + parsedTime.TimeOfDay,
                            LogType = logType,
                            DeviceName = string.IsNullOrWhiteSpace(deviceName) ? null : deviceName,
                            ProjectName = projectName.Trim(),
                            ImportSource = "File",
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Manuel"
                        });
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"Invalid data format at row {row}: {ex.Message}");
                    }
                }

                // Bulk insert with progress
                if (biometricsLogs.Any())
                {
                    // Create a progress reporter
                    var progress = new Progress<(int Processed, int Total, string Status)>();
                    var progressUpdates = new List<string>();

                    progress.ProgressChanged += (sender, update) =>
                    {
                        progressUpdates.Add($"Processed {update.Processed}/{update.Total} - {update.Status}");
                    };

                    var inserted = await _biometricsLogService.BulkInsertWithProgressAsync(biometricsLogs, progress);

                    stopwatch.Stop();

                    return Ok(new
                    {
                        TotalProcessed = biometricsLogs.Count,
                        Inserted = inserted,
                        Failed = biometricsLogs.Count - inserted,
                        Errors = errors,
                        ProgressUpdates = progressUpdates,
                        ImportTimeMs = stopwatch.ElapsedMilliseconds,
                        RecordsPerSecond = biometricsLogs.Count / (stopwatch.ElapsedMilliseconds / 1000.0)
                    });
                }

                return Ok(new
                {
                    Message = "No valid records to import",
                    Total = 0,
                    Errors = errors
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }
    }
}
