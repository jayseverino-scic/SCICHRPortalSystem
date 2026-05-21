using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Drawing.Printing;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Settings;
using SCICHRPortal.Data.TimekeepingTables;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class TimekeepingDevicesController : ControllerBase
    {
        private ITimekeepingDevicesService TimekeepingDevicesService { get; }
        public TimekeepingDevicesController(ITimekeepingDevicesService timekeepingDevicesService)
        {
            TimekeepingDevicesService = timekeepingDevicesService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var timekeepingDevices = await TimekeepingDevicesService.GetAllAsync();
            return Ok(timekeepingDevices);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await TimekeepingDevicesService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.Id,
                d.Name,
                d.SerialNumber,
                IsTodayAnnouncement = dateToday.Date == d.CreatedAt.Date,
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
        public async Task<IActionResult> InsertAsync(TimekeepingDevices timekeepingDevices)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await TimekeepingDevicesService.HasDuplicateName(timekeepingDevices);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            timekeepingDevices.CreatedBy = "manuel";
            timekeepingDevices.CreatedAt = DateTime.Now;
            await TimekeepingDevicesService.InsertAsync(timekeepingDevices);

            return StatusCode(201, timekeepingDevices.Id);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(TimekeepingDevices timekeepingDevices)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            timekeepingDevices.UpdatedAt = DateTime.Now;
            timekeepingDevices.UpdatedBy = "manuel";
            var updated = await TimekeepingDevicesService.UpdateAsync(timekeepingDevices);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{timekeepingDevicesId}")]
        public async Task<IActionResult> DeleteAsync(int Id)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await TimekeepingDevicesService.DeleteAsync(Id);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
        [Authorize]
        [HttpPost("ImportDevices")]
        public async Task<IActionResult> ImportDevices()
        {
            IEnumerable<ZKDevices> devices = await TimekeepingDevicesService.GetDevices();

            foreach (var device in devices)
            {
                TimekeepingDevices existingDevice = await TimekeepingDevicesService.GetBySerialNumber(device.SerialNumber);
                if (existingDevice == null)
                {
                    TimekeepingDevices item = new TimekeepingDevices
                    {
                        Name = device.Name,
                        SerialNumber = device.SerialNumber,
                        CreatedAt = DateTime.Now,
                        CreatedBy = "jun rivas"
                    };
                    await TimekeepingDevicesService.InsertAsync(item);
                }
                else
                {
                    TimekeepingDevices item = new TimekeepingDevices
                    {
                        Id = existingDevice.Id,
                        Name = device.Name,
                        SerialNumber = device.SerialNumber,
                        UpdatedAt = DateTime.Now,
                        UpdatedBy = "jun rivas"
                    };
                    await TimekeepingDevicesService.UpdateAsync(item);
                }
            }
            var timekeepingDevices = await TimekeepingDevicesService.GetAllAsync();
            return Ok(timekeepingDevices);
        }
    }
}
