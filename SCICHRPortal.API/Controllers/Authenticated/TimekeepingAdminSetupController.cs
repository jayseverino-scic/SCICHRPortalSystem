using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class TimekeepingAdminSetupController : ControllerBase
    {
        public ITimekeepingAdminSetupService TimekeepingAdminSetupService { get;}
        public TimekeepingAdminSetupController(ITimekeepingAdminSetupService timekeepingAdminSetupService)
        {
            TimekeepingAdminSetupService = timekeepingAdminSetupService;
        }
        [HttpGet]
        public async Task<IActionResult> GetSettingsAsync()
        {
            var setting = await TimekeepingAdminSetupService.GetAsync(1);
            return Ok(setting);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateSettingsAsync(TimekeepingAdminSetup setting)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var settingOnDB = await TimekeepingAdminSetupService.GetAsync(1);

            if (settingOnDB is not null)
            {
                settingOnDB.ShiftLateMinuteGracePeriod = setting.ShiftLateMinuteGracePeriod;
                settingOnDB.BreakLateMinuteGracePeriod = setting.BreakLateMinuteGracePeriod;
                settingOnDB.ShiftLateTotalMinuteLimit = setting.ShiftLateTotalMinuteLimit;
                settingOnDB.BreakLateTotalMinuteLimit = setting.BreakLateTotalMinuteLimit;
                settingOnDB.NoTimeLogCountLimit = setting.NoTimeLogCountLimit;
                settingOnDB.NoLeaveAbsentCountLimit = setting.NoLeaveAbsentCountLimit;
                await TimekeepingAdminSetupService.UpdateAsync(settingOnDB);
            }
            return Ok();
        }
    }
}
