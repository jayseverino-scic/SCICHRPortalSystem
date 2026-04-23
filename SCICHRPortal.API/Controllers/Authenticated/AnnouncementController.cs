using BrunoZell.ModelBinding;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class AnnouncementController : ControllerBase
    {
        private IAnnouncementService AnnouncementService { get; }
        private AppSettings AppSettings { get; }
        private readonly IMailService MailService;
        public AnnouncementController(IAnnouncementService announcementService,
            IOptions<AppSettings> appSettings,
            IMailService mailService)
        {
            AnnouncementService = announcementService;
            AppSettings = appSettings.Value;
            MailService = mailService;
        }

        #region Private Method
        private async Task<FileStreamResult> GetEmailTemplate(string templateUrl)
        {

            using HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(AppSettings.WebUrl!)
            };

            HttpResponseMessage response = await client.GetAsync(templateUrl);

            if (response.IsSuccessStatusCode)
            {
                var content = response.Content;
                var contentStream = await content.ReadAsStreamAsync();
                return File(contentStream, "text/html", "Template.html");
            }
            else
            {
                throw new FileNotFoundException();
            }
        }
        #endregion

        [Authorize]
        [HttpGet()]
        public async Task<IActionResult> GetAsync()
        {
            var announcements = await AnnouncementService.GetAllAsync();

            return Ok(announcements);
        }

        [Authorize]
        [HttpGet("Filter")]
        public async Task<IActionResult> FilterAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await AnnouncementService.FilterAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;
            var dateToday = DateTime.Today;

            var data = tuple.Item1.Select(d => new
            {
                d.AnnouncementId,
                d.Title,
                d.AnnouncementForm,
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

        [Authorize]
        [HttpPost()]
        public async Task<IActionResult> InsertAsync([ModelBinder(BinderType = typeof(JsonModelBinder))] Announcement announcement, IFormFile? file, [FromForm] List<int>? roleIds, [FromForm] List<int>? sectionIds, [FromForm] bool notifyStudents = false)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var hasDuplicate = await AnnouncementService.HasDuplicateName(announcement);
            if (hasDuplicate.IsDuplicated)
                return Conflict(hasDuplicate);
            announcement.CreatedAt = DateTime.Now;
            announcement.CreatedBy = "manuel";
            await AnnouncementService.InsertAsync(announcement);


            var selectedRoleIds = roleIds?.Where(x => x > 0).Distinct().ToList() ?? new List<int>();
            var selectedSectionIds = sectionIds?.Where(x => x > 0).Distinct().ToList() ?? new List<int>();
            //var recipients = await AnnouncementService.GetAllEmailByRoleIdsAsync(selectedRoleIds, notifyStudents, selectedSectionIds);

            //if (recipients.Any())
            //{
            //    foreach (var recipient in recipients)
            //    {
            //        var emailTemplate = await GetEmailTemplate(AppSettings.WebUrl + "html/templates/AnnouncementTemplate.html");
            //        await MailService.SendAnnouncementEmailAsync(recipient.Email,
            //                  recipient.Name,
            //                  announcement.Title!,
            //                  announcement.AnnouncementForm!,
            //                  file!,
            //                  emailTemplate);
            //    }
            //}

            return StatusCode(201, announcement.AnnouncementId);
        }

        [Authorize]
        [HttpPut()]
        public async Task<IActionResult> UpdateAsync(Announcement announcement)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            announcement.UpdatedAt = DateTime.Now;
            announcement.UpdatedBy = "manuel";
            var updated = await AnnouncementService.UpdateAsync(announcement);
            if (!updated)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        [Authorize]
        [HttpDelete("{announcementId}")]
        public async Task<IActionResult> DeleteAsync(int announcementId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await AnnouncementService.DeleteAsync(announcementId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }
    }
}
