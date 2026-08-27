using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class BiometricsLogController : ControllerBase
    {
        private readonly IBiometricsLogService BiometricsLogService { get; }
        private readonly IEmployeeService EmployeeService { get; }
        private readonly ISPersonnelsService PersonnelsService { get; }
        private readonly IBiometricsBulkService _bulkService;

        // Bulk import configuration
        private const int BULK_BATCH_SIZE = 5000;
        private const int EMPLOYEE_CHUNK_SIZE = 1000;

        public BiometricsLogController(
            IBiometricsLogService biometricsLogService, 
            IEmployeeService employeeService, 
            ISPersonnelsService personnelsService,
            IBiometricsBulkService bulkService)
        {
            BiometricsLogService = biometricsLogService;
            EmployeeService = employeeService;
            PersonnelsService = personnelsService;
            _bulkService = bulkService;
        }

        // Existing GET methods remain unchanged...
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var biometricsLogs = await BiometricsLogService.GetAllAsync();
            return Ok(biometricsLogs);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword, DateTime? startDate, DateTime? endDate, string? deviceName)
        {
            var tuple = await BiometricsLogService.FilterAsync(pageNumber, pageSize, searchKeyword!, startDate, endDate, deviceName);
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
            var tuple = await BiometricsLogService.FilterByProjectAndDateRange(startDate, endDate, projectName);

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

        [HttpPost()]
        public async Task<IActionResult> InsertAsync(BiometricsLog biometricsLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            await BiometricsLogService.InsertAsync(biometricsLog);
            return StatusCode(201, biometricsLog.BiometricsLogId);
        }

        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(BiometricsLog biometricsLog)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var updated = await BiometricsLogService.UpdateAsync(biometricsLog);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        // ============ OPTIMIZED IMPORT METHODS ============

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

            try
            {
                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);

                using var package = new ExcelPackage(stream);
                ExcelWorksheet workSheet = package.Workbook.Worksheets[0];
                var rowCount = workSheet.Dimension.Rows;

                // Process all rows first to build the list
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

                        BiometricsLog biometricsLog = new()
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
                        };
                        biometricsLogs.Add(biometricsLog);
                    }
                    catch (Exception)
                    {
                        return StatusCode(422, $"Invalid data format at row {row}");
                    }
                }

                // Bulk insert all records at once
                if (biometricsLogs.Any())
                {
                    await _bulkService.BulkInsertBiometricsLogsAsync(biometricsLogs);
                }

                stopwatch.Stop();

                var dto = new
                {
                    Data = biometricsLogs,
                    Total = biometricsLogs.Count,
                    ImportTimeMs = stopwatch.ElapsedMilliseconds,
                    RecordsPerSecond = biometricsLogs.Count / (stopwatch.ElapsedMilliseconds / 1000.0)
                };

                return Ok(dto);
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
        public async Task<ActionResult> ImportDb(DateTime? startImport, DateTime? endImport, string? serialNumber)
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                // Step 1: Fetch time logs from source
                var timeLogs = await BiometricsLogService.ImportDbDateRange(startImport, endImport, serialNumber);
                
                if (timeLogs == null || !timeLogs.Any())
                {
                    return Ok(new 
                    { 
                        Message = "No records found to import",
                        Total = 0 
                    });
                }

                // Step 2: Get all unique employee numbers
                var employeeNumbers = timeLogs
                    .Where(t => !string.IsNullOrEmpty(t.AccessNumber))
                    .Select(t => t.AccessNumber!)
                    .Distinct()
                    .ToList();

                // Step 3: Bulk fetch employees in chunks
                var employeeDict = await GetEmployeesInBulkAsync(employeeNumbers);

                // Step 4: Prepare biometrics logs
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

                // Step 5: Bulk insert all records
                if (biometricsLogs.Any())
                {
                    await _bulkService.BulkInsertBiometricsLogsAsync(biometricsLogs);
                }

                stopwatch.Stop();

                var dto = new
                {
                    TotalFetched = timeLogs.Count,
                    ProcessedCount = biometricsLogs.Count,
                    SkippedCount = skippedCount,
                    Total = biometricsLogs.Count,
                    ImportTimeMs = stopwatch.ElapsedMilliseconds,
                    RecordsPerSecond = biometricsLogs.Count / (stopwatch.ElapsedMilliseconds / 1000.0)
                };

                return Ok(dto);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Import failed: {ex.Message}");
            }
        }

        // Helper method to bulk fetch employees
        private async Task<Dictionary<string, SPersonnels>> GetEmployeesInBulkAsync(List<string> employeeNumbers)
        {
            var result = new Dictionary<string, SPersonnels>();

            for (int i = 0; i < employeeNumbers.Count; i += EMPLOYEE_CHUNK_SIZE)
            {
                var chunk = employeeNumbers.Skip(i).Take(EMPLOYEE_CHUNK_SIZE).ToList();
                var employees = await PersonnelsService.GetByMultipleSPersonnelsNoAsync(chunk);

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