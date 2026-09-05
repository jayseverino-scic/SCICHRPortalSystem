using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class BiometricsLogController : ControllerBase
    {
        private readonly IBiometricsLogService _biometricsLogService;

        public BiometricsLogController(IBiometricsLogService biometricsLogService)
        {
            _biometricsLogService = biometricsLogService;
        }

        // ============ EXISTING GET METHODS ============

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
        public async Task<ActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null)
                return BadRequest(ResponseMessage.BadRequest);

            var extension = Path.GetExtension(file.FileName);
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
                            DeviceName = deviceName,
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
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Step 1: Fetch time logs from TimekeepingContext - FIXED: Now returns List<TimeLog>
                var timeLogs = await _biometricsLogService.ImportDbDateRange(startImport, endImport, projectName);

                // FIXED: Use Count property instead of Any() for List<T>
                if (timeLogs == null || timeLogs.Count == 0)
                {
                    return Ok(new
                    {
                        Message = "No records found to import",
                        Total = 0
                    });
                }

                // Step 2: Get all unique employee numbers - FIXED: Properly extract AccessNumber as string
                var employeeNumbers = timeLogs
                    .Where(t => !string.IsNullOrEmpty(t.AccessNumber))
                    .Select(t => t.AccessNumber)  // This is now a string
                    .Distinct()
                    .ToList();

                // Step 3: Bulk fetch employees from XscribeContext
                var employeeDict = await _biometricsLogService.GetEmployeesInBulkAsync(employeeNumbers);

                // Step 4: Prepare biometrics logs
                var biometricsLogs = new List<BiometricsLog>();
                var skippedCount = 0;

                foreach (var timeLog in timeLogs)
                {
                    if (employeeDict.TryGetValue(timeLog.AccessNumber, out var employee))
                    {
                        biometricsLogs.Add(new BiometricsLog
                        {
                            PersonnelId = timeLog.AccessNumber,
                            LastName = employee.LastName ?? string.Empty,
                            FirstName = employee.FirstName ?? string.Empty,
                            Date = timeLog.RecordDate,
                            Time = Convert.ToDateTime(Convert.ToString(timeLog.TimeLogStamp)),
                            LogType = timeLog.LogType?.ToString() ?? string.Empty,
                            DeviceName = timeLog.DeviceSerialNumber ?? string.Empty,
                            ProjectName = projectName ?? string.Empty,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Manuel"
                        });
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                // Step 5: Bulk insert all records
                if (biometricsLogs.Any())
                {
                    var result = await _biometricsLogService.BulkInsertWithResultAsync(biometricsLogs);

                    stopwatch.Stop();

                    return Ok(new
                    {
                        TotalFetched = timeLogs.Count,  // FIXED: Now works with List<T>
                        ProcessedCount = biometricsLogs.Count,
                        SkippedCount = skippedCount,
                        Inserted = result.Inserted,
                        Failed = result.Failed,
                        Errors = result.Errors,
                        ImportTimeMs = stopwatch.ElapsedMilliseconds,
                        RecordsPerSecond = biometricsLogs.Count / (stopwatch.ElapsedMilliseconds / 1000.0)
                    });
                }

                stopwatch.Stop();
                return Ok(new
                {
                    TotalFetched = timeLogs.Count,  // FIXED: Now works with List<T>
                    ProcessedCount = 0,
                    SkippedCount = skippedCount,
                    Message = "No valid records to import"
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
        public async Task<ActionResult> UploadFileWithProgressAsync(IFormFile file)
        {
            if (file == null)
                return BadRequest(ResponseMessage.BadRequest);

            var extension = Path.GetExtension(file.FileName);
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
                            DeviceName = deviceName,
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