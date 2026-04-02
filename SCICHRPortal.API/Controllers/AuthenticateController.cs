using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SCICHRPortal.API.Models.RequestModels.Authenticate;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Users;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Cryptography;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private IUserService UserService { get; }
        private IAccessTokenService AccessTokenService { get; }
        private readonly IMailService MailService;
        private AppSettings AppSettings { get; }

        public AuthenticateController
        (
            IUserService userService,
            IAccessTokenService accessTokenService,
            IOptions<AppSettings> appSettings,
            IMailService mailService
        )
        {
            UserService = userService;
            AccessTokenService = accessTokenService;
            AppSettings = appSettings.Value;
            MailService = mailService;
        }


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


        [HttpPost("User")]
        public async Task<IActionResult> Authenticate(LoginRequestModel request)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var user = await UserService.GetByEmailAsync(request.Email!);

            if (user == null)
                return Unauthorized(ResponseMessage.InvalidCredentials);

            if (!user!.IsApproved)
                return Forbid();

            if (!user!.Active)
                return Forbid();

            var valid = Hash.Validate(request.Password!, user.Salt!, user.Password!);

            if (!valid || user.Locked)
            {
                user.LoginAttempts++;
                await UserService.SaveChanges();

                if (user.LoginAttempts >= 10 && user.UserId != 1)
                {
                    user.Locked = true;
                    await UserService.SaveChanges();
                    return Conflict(ResponseMessage.LoginAttemptsExceeded);
                }
                else
                {
                    return Unauthorized(ResponseMessage.InvalidCredentials);
                }
            }

            if(user.Locked)
                return Unauthorized(ResponseMessage.InvalidCredentials);

            IEnumerable<UserRole> userRoles = await UserService.GetUserRolesAsync(user.UserId);

            var accessToken = await AccessTokenService.Generate(user, userRoles.Select(ur => ur.Role!));

            user.LoginAttempts = 0;
            await UserService.SaveChanges();

            var dto = new
            {
                accessToken,
                user.IsPasswordChanged
            };

            return Ok(dto);
        }


        [HttpPost("User/ForgotPassword")]
        public async Task<IActionResult> ResetPassword(ForgotPasswordRequestModel request)
        {
            var user = await UserService.GetByEmailAsync(request.Email!);

            if (user == null)
                return NotFound(ResponseMessage.NotFound);

            string randomPassword = Guid.NewGuid().ToString("N").ToLower()
                      .Replace("1", "").Replace("o", "").Replace("0", "")
                      .Substring(0, 10);

            var salt = Salt.Create();

            user.Password = Hash.Create(randomPassword, salt);
            user.Salt = salt;
            user.IsPasswordChanged = false;
            user.Locked = false;
            user.LoginAttempts = 0;

            await UserService.ResetPassword(user);

            //NOTE: call service here to send the new password to the user
            try
            {
                var emailTemplate = await GetEmailTemplate(AppSettings.WebUrl + "html/templates/NewPasswordTemplate.html");
                    await MailService.SendForgotPasswordEmailAsync(user.Email!, $"{user.LastName}, {user.FirstName}", randomPassword, emailTemplate);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
            return Ok();
        }


    }
}
