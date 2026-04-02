using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;

namespace SCICHRPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupsController : ControllerBase
    {
        private ILookupsService LookupsService { get; }
        public LookupsController(ILookupsService lookupsService)
        {
            LookupsService = lookupsService;
        }

        
        [HttpGet("Module")]
        public async Task<IActionResult> ModuleAsync()
        {
            return Ok(await LookupsService.GetAll<Module>());
        }

        [HttpGet("Module/Filter")]
        public async Task<IActionResult> FilterModuleAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await LookupsService.FilterModuleAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.ModuleId,
                d.Description,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };

            return Ok(dto);
        }

        [HttpPost("Module")]
        public async Task<IActionResult> InsertModuleAsync(Module module)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await LookupsService.HasDuplicateName(module);
            if (hasDuplicate)
                return Conflict(new { Message = "Module Duplicated" });

            await LookupsService.InsertAsync(module);

            return StatusCode(201, module.ModuleId);
        }

        [HttpPut("Module")]
        public async Task<IActionResult> UpdateModuleAsync(Module module)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            await LookupsService.UpdateAsync(module);

            return Ok();
        }

        [HttpDelete("Module/{moduleId}")]
        public async Task<IActionResult> RemoveModuleAsync(int moduleId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await LookupsService.DeleteModuleTypeAsync(moduleId);

            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }

}
