using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch.Internal;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Extensions;
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
        private IBiometricsLogService BiometricsLogService { get; }
        private IEmployeeService EmployeeService { get; }
        private ISPersonnelsService PersonnelsService { get; }
        private ITimekeepingDevicesService TimekeepingDevicesService { get; }

        public BiometricsLogController(IBiometricsLogService biometricsLogService, IEmployeeService employeeService, ITimekeepingDevicesService timekeepingDevicesService, ISPersonnelsService personnelsService)
        {
            BiometricsLogService = biometricsLogService;
            EmployeeService = employeeService;
            TimekeepingDevicesService = timekeepingDevicesService;
            PersonnelsService = personnelsService;
        }
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
        public async Task<IActionResult> FilterPerProjectAsync(int pageNumber, int pageSize, string? searchKeyword, DateTime? startDate, DateTime? endDate, string? projectName)
        {
            var tuple = await BiometricsLogService.FilterAsync(pageNumber, pageSize, searchKeyword!, startDate, endDate, projectName);
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
        [HttpPost("Import")]
        [Consumes("multipart/form-data")]
        [Authorize]
        public async Task<ActionResult> UploadFileAsync(IFormFile file)
        {
            if (file == null)
                return BadRequest(ResponseMessage.BadRequest);

            var extension = Path.GetExtension(file.FileName);
            if (extension != ".xls")
            {
                return StatusCode(415, ResponseMessage.FileNotSupported);
            }
            var biometricsLogs = new List<BiometricsLog>();
            using (var stream = new MemoryStream())
            {
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
                        var firstNameName = workSheet.Cells[row, 3].Value?.ToString()?.Trim() ?? "";
                        var date = workSheet.Cells[row, 4].Value?.ToString()?.Trim() ?? "";
                        var time = workSheet.Cells[row, 5].Value?.ToString()?.Trim() ?? "";
                        var logType = workSheet.Cells[row, 6].Value?.ToString()?.Trim() ?? "";
                        var deviceName = workSheet.Cells[row, 7].Value?.ToString()?.Trim() ?? "";


                        //if (string.IsNullOrWhiteSpace(employeeNo) || string.IsNullOrWhiteSpace(employeeName) || string.IsNullOrWhiteSpace(inOut)
                        //    || string.IsNullOrWhiteSpace(dateTimeLog))
                        //    return StatusCode(422, $"One or more fields invalid at row {row}");

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
                            FirstName = firstNameName,
                            Date = parsedDate,
                            Time = Convert.ToDateTime(parsedDate.ToShortDateString() + " " + parsedTime.ToShortTimeString()),
                            LogType = logType.ToString(),
                            DeviceName = deviceName,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Manuel"
                        };
                        biometricsLogs.Add(biometricsLog);
                        await BiometricsLogService.InsertAsync(biometricsLog);
                    }
                    catch (Exception)
                    {
                        return StatusCode(422, $"One or more fields invalid at row {row}");
                    }
                }
            }
            var dto = new
            {
                Data = biometricsLogs,
                Total = biometricsLogs.Count()
            };
            return Ok(dto);
        }

        [HttpGet("ImportDb")]
        [Authorize]
        public async Task<ActionResult> ImportDb(DateTime? startImport, DateTime? endImport, string? serialNumber)
        {
            if (!startImport.HasValue || !endImport.HasValue)
                return BadRequest(ResponseMessage.BadRequest);

            TimekeepingDevices timekeepingDevice = await TimekeepingDevicesService.GetBySerialNumber(serialNumber);
            string? deviceName = "";
            if (timekeepingDevice != null)
            {
                deviceName = timekeepingDevice.Name;
            }
            var timeLogs = await BiometricsLogService.ImportDbDateRange(startImport, endImport, serialNumber);
            var biometricsLogs = new List<BiometricsLog>();
            if (timeLogs != null)
            {
                foreach (var timeLog in timeLogs)
                {
                    SPersonnels employee = await PersonnelsService.GetBySPersonnelsNoAsync(timeLog.AccessNumber!);
                    if (employee != null)
                    {
                        BiometricsLog biometricsLog = new()
                        {
                            PersonnelId = timeLog.AccessNumber,
                            LastName = employee.LastName,
                            FirstName = employee.FirstName,
                            Date = timeLog.RecordDate,
                            Time = Convert.ToDateTime(Convert.ToString(timeLog.TimeLogStamp)),
                            LogType = timeLog.LogType!.ToString(),
                            DeviceName = deviceName,
                            ProjectName = serialNumber,
                            CreatedAt = DateTime.Now,
                            CreatedBy = "Manuel"
                        };
                        biometricsLogs.Add(biometricsLog);
                        await BiometricsLogService.InsertAsync(biometricsLog);
                    }
                }
            }
            var dto = new
            {
                Data = biometricsLogs,
                Total = biometricsLogs.Count()
            };
            return Ok(dto);
        }
    }
}
