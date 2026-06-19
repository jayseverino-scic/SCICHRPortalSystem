using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Data.Entities.Metadatas;
using SCICHRPortal.Data.Enums;
using SCICHRPortal.Data.TimekeepingTables;
using SCICHRPortal.Data.XscribeTables;
using SCICHRPortal.Service.Implementations;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Settings;
using System.Drawing.Printing;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Authorize]
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class CompanyBranchController : ControllerBase
    {
        private ICompanyBranchService CompanyBranchService { get; }
        public CompanyBranchController(ICompanyBranchService companyBranchService)
        {
            CompanyBranchService = companyBranchService;
        }
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var companyBranch = await CompanyBranchService.GetAllAsync();
            return Ok(companyBranch);
        }

        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await CompanyBranchService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.Id,
                d.Name,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };
            return Ok(dto);
        }
    }
}
