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

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private IDeviceService DeviceService { get; }
        private ISZKDevicesService SZKDevicesService { get; }
        public DeviceController(IDeviceService deviceService, ISZKDevicesService szkDevicesService)
        {
            DeviceService = deviceService;
            SZKDevicesService = szkDevicesService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var device = await DeviceService.GetAllAsync();
            return Ok(device);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await DeviceService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.Id,
                d.SerialNumber,
                d.Name,
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
        public async Task<IActionResult> InsertAsync(Device device)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await DeviceService.HasDuplicateName(device);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            device.CreatedAt = DateTime.Now;
            device.CreatedBy = "manuel";
            await DeviceService.InsertAsync(device);

            return StatusCode(201, device.Id);
        }


        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Device device)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            device.UpdatedBy = "manuel";
            device.UpdatedAt = DateTime.Now;
            var updated = await DeviceService.UpdateAsync(device);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }


        [HttpDelete("{deviceId}")]
        public async Task<IActionResult> DeleteAsync(int deviceId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await DeviceService.DeleteAsync(deviceId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
        [HttpGet("ImportDb")]
        [Authorize]
        public async Task<ActionResult> ImportDb()
        {
            var szkDevices = await SZKDevicesService.GetAllAsync();
            var existingDevices = await DeviceService.GetAllAsync();
            var devicesList = new List<Device>();
            if (szkDevices != null)
            {
                foreach (var device in szkDevices)
                {
                    Device deviceQuery = existingDevices.Where(d => d.SerialNumber?.ToUpper() == device.SerialNumber?.ToUpper()).SingleOrDefault();
                    if (deviceQuery == null)
                    {
                        Device newDevice = new()
                        {
                            Name = device.Name,
                            SerialNumber = device.SerialNumber,
                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = "manuel"

                        };
                        devicesList.Add(newDevice);
                        await DeviceService.InsertAsync(newDevice);
                    }
                }
            }
            var dto = new
            {
                Data = devicesList,
                Total = devicesList.Count()
            };
            return Ok(dto);
        }
    }
}
