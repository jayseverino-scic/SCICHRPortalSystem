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
        private readonly ISPersonnelsService _personnelsService;
        private const int EMPLOYEE_CHUNK_SIZE = 1000;

        public BiometricsLogController(
            IBiometricsLogService biometricsLogService,
            ISPersonnelsService personnelsService)
        {
            _biometricsLogService = biometricsLogService;
            _personnelsService = personnelsService;
        }

        // All existing GET, POST, PUT methods remain the same...

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

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                var rowCount = workSheet.Dimension.Rows;

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
                            return StatusCode(422, $"Date or time missing at row {row}");
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
                    catch (Exception)
                    {
                        return StatusCode(422, $"Invalid data format at row {row}");
                    }
                }

                if (biometricsLogs.Any())
                {
                    var result = await _biometricsLogService.BulkInsertWithResultAsync(biometricsLogs);

                    stopwatch.Stop();

                    return Ok(new
                    {
                        Data = biometricsLogs,
                        Total = biometricsLogs.Count,
                        Result = result,
                        ImportTimeMs = stopwatch.ElapsedMilliseconds,
                        RecordsPerSecond = biometricsLogs.Count / (stopwatch.ElapsedMilliseconds / 1000.0)
                    });
                }

                return Ok(new { Message = "No records to import", Total = 0 });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }

        [HttpGet("ImportDb")]
        [Authorize]
        public async Task<ActionResult> ImportDb(DateTime? startImport, DateTime? endImport, string? serialNumber)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                var timeLogs = await _biometricsLogService.ImportDbDateRange(startImport, endImport, serialNumber);

                if (timeLogs == null || !timeLogs.Any())
                {
                    return Ok(new { Message = "No records found to import", Total = 0 });
                }

                var employeeNumbers = timeLogs
                    .Where(t => !string.IsNullOrEmpty(t.AccessNumber))
                    .Select(t => t.AccessNumber!)
                    .Distinct()
                    .ToList();

                var employeeDict = await GetEmployeesInBulkAsync(employeeNumbers);

                var biometricsLogs = new List<BiometricsLog>(timeLogs.Count);
                var skippedCount = 0;

                foreach (var timeLog in timeLogs)
                {
                    if (employeeDict.TryGetValue(timeLog.AccessNumber!, out var employee))
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
                            ProjectName = serialNumber ?? string.Empty,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Manuel"
                        });
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                if (biometricsLogs.Any())
                {
                    var result = await _biometricsLogService.BulkInsertWithResultAsync(biometricsLogs);

                    stopwatch.Stop();

                    return Ok(new
                    {
                        TotalFetched = timeLogs.Count,
                        ProcessedCount = biometricsLogs.Count,
                        SkippedCount = skippedCount,
                        Result = result,
                        ImportTimeMs = stopwatch.ElapsedMilliseconds,
                        RecordsPerSecond = biometricsLogs.Count / (stopwatch.ElapsedMilliseconds / 1000.0)
                    });
                }

                stopwatch.Stop();
                return Ok(new
                {
                    TotalFetched = timeLogs.Count,
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

        private async Task<Dictionary<string, SPersonnels>> GetEmployeesInBulkAsync(List<string> employeeNumbers)
        {
            var result = new Dictionary<string, SPersonnels>();

            for (int i = 0; i < employeeNumbers.Count; i += EMPLOYEE_CHUNK_SIZE)
            {
                var chunk = employeeNumbers.Skip(i).Take(EMPLOYEE_CHUNK_SIZE).ToList();
                var employees = await _personnelsService.GetByMultipleSPersonnelsNoAsync(chunk);

                foreach (var employee in employees)
                {
                    if (!result.ContainsKey(employee.PersonnelNo))
                    {
                        result[employee.PersonnelNo] = employee;
                    }
                }
            }

            return result;
        }
    }
}