using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class AuditController : ControllerBase
    {
        private IAuditService AuditService { get; }
        public AuditController(IAuditService auditService)
        {
            AuditService = auditService;
        }


        [Authorize]
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword, string? system)
        {
            var tuple = await AuditService.FilterAsync(pageNumber, pageSize, searchKeyword!, system!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.Id,
                d.UserName,
                d.Type,
                d.TableName,
                d.RequestOrigin,
                d.DateTime,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpGet("HasRecord")]
        public async Task<IActionResult> HasRecordAsyncAsync(string tableName, string type, DateTime dateTime)
        { 
            var hasRecord = await AuditService.HasRecord(tableName, type, dateTime);

            return Ok(hasRecord);
        }
       
    }
}
