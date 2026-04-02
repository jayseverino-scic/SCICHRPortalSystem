using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Users;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Cryptography;
using SCICHRPortal.Utility.Settings;

namespace SCICHRPortal.API.Controllers.Authenticated
{
    [Route("api/Authenticated/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private IUserService UserService { get; }
        private IUserRoleService UserRoleService { get; }
        private IRoleService RoleService { get; }
        private ILookupsService LookupsService { get; }
        private readonly IMailService MailService;
        private AppSettings AppSettings { get; }

        public UserController(IUserService userService, IUserRoleService userRoleService,
            IRoleService roleService,
             IOptions<AppSettings> appSettings,
            IMailService mailService,
            ILookupsService lookupsService)
        {
            UserService = userService;
            UserRoleService = userRoleService;
            RoleService = roleService;
            AppSettings = appSettings.Value;
            MailService = mailService;
            LookupsService = lookupsService;
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


        private async Task SendNewPassword(User user)
        {

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
                BadRequest(ex.Message);
            }
        }


        [Authorize]
        [HttpGet("role/{roleId}")]
        public async Task<IActionResult> GetByRoleId(int roleId)
        {
            var user = await UserService.GetByRoleIdsync(roleId);
            var dto = user.Select(u => new
            {
                u.UserId,
                FullName = $"{u.User!.LastName}, {u.User!.FirstName}",
                u.User.Email,
                u.User.Username,
            });
            return Ok(dto);
        }

        [Authorize]
        [HttpGet("Email/{email}")]
        public async Task<IActionResult> GetByRoleId(string email)
        {
            var user = await UserService.GetByEmailAsync(email);
            return Ok(user);
        }

        [HttpGet("GetAuthenticatedAsync")]
        [Authorize]
        public async Task<IActionResult> GetAuthenticatedAsync()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.Sid));
            var user = await UserService.GetAsync(userId);

            if (user == null)
                return NotFound(ResponseMessage.NotFound);

            var userRole = await UserRoleService.GetByUserIdAsync(userId);

            var roleFromDb = await RoleService.GetAsync(userRole.RoleId);
            var dto = new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                Name = $"{user.LastName}, {user.FirstName}",
                user.Username,
                user.Email,
                user.ContactNumber,
                user.MiddleName,
                RoleName = roleFromDb.Name
            };

            return Ok(dto);
        }

        [Authorize]
        [HttpGet("Unapproved/Filter")]
        public async Task<IActionResult> GetUnApprovedUserAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await UserService.FilterUnApprovedUserAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.UserId,
                d.FirstName,
                d.LastName,
                d.Username,
                d.MiddleName,
                d.Email,
                d.ContactNumber,
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
        [HttpGet("Approved/Filter")]
        public async Task<IActionResult> GetApprovedUserAsync(int pageNumber, int pageSize, string? searchKeyword)
        {
            var tuple = await UserService.FilterApprovedUserAsync(pageNumber, pageSize, searchKeyword!);
            var maxOrderNumber = pageNumber * pageSize;
            var orderNumber = maxOrderNumber - pageSize + 1;

            var data = tuple.Item1.Select(d => new
            {
                d.UserId,
                d.FirstName,
                d.LastName,
                d.Username,
                d.MiddleName,
                d.Email,
                d.ContactNumber,
                d.UserRoles,
                d.Active,
                OrderNumber = orderNumber++
            });

            var dto = new
            {
                Data = data,
                Total = tuple.Item2
            };

            return Ok(dto);
        }

        [HttpPost("Register")]
        //[Authorize]
        public async Task<IActionResult> PostAsync(InsertUserRequestModel request)
        {
            var access = HttpContext.Response.Headers.SingleOrDefault(h => h.Key == "Set-Cookie").Value.Select(h => h).FirstOrDefault();

            if (access != null && access.Contains("accessDenied=true"))
            {
                HttpContext.Response.Cookies.Delete("accessDenied");
                return StatusCode(403);
            }

            if (!ModelState.IsValid)
                return BadRequest(ResponseMessage.BadRequest);

            var salt = Salt.Create();

            string randomPassword = Guid.NewGuid().ToString("N").ToLower()
                      .Replace("1", "").Replace("o", "").Replace("0", "")
                      .Substring(0, 10);

            User user = new()
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                MiddleName = request.MiddleName,
                Email = request.Email,
                Salt = salt,
                Password = Hash.Create(randomPassword, salt),
                ContactNumber = request.ContactNumber,
                IsPasswordChanged = false,
                IsApproved = request.IsApproved.HasValue? request.IsApproved.Value: false,
                UserRoles = request.UserRoles,
            };


            var duplicateEmail = await UserService.GetDuplicateEmailAsync(user);

            if (duplicateEmail is not null)
                return Conflict(new { Message = "Email Duplicated" });

            await UserService.InsertAsync(user);

            //UserRole userRole = new()
            //{
            //    UserId = user.UserId,
            //    RoleId = request.RoleId
            //};

            //await UserRoleService.InsertAsync(userRole);

            if (user.IsApproved)
            {
                try
                {
                    var emailTemplate = await GetEmailTemplate(AppSettings.WebUrl + "html/templates/NewUserTemplate.html");
                    await MailService.SendForgotPasswordEmailAsync(user.Email!, $"{user.LastName}, {user.FirstName}", randomPassword, emailTemplate);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }


            return StatusCode(201, user.UserId);
        }

        [HttpPut("{userId}")]
        [Authorize]
        public async Task<IActionResult> PutAsync(int userId, UpdateUserRequestModel request)
        {
            var access = HttpContext.Response.Headers.SingleOrDefault(h => h.Key == "Set-Cookie").Value.Select(h => h).FirstOrDefault();

            if (access != null && access.Contains("accessDenied=true"))
            {
                HttpContext.Response.Cookies.Delete("accessDenied");
                return StatusCode(403);
            }

            if (!ModelState.IsValid)
                return BadRequest(ResponseMessage.BadRequest);

            if (userId != request.UserId)
                return BadRequest(ResponseMessage.BadRequest);

            var userRecord = await UserService.GetAsync(userId);

            if (userRecord == null)
                return NotFound(ResponseMessage.NotFound);


            //userRecord.Username = request.Username;
            userRecord.FirstName = request.FirstName;
            userRecord.MiddleName = request.MiddleName;
            userRecord.LastName = request.LastName;
            userRecord.ContactNumber = request.ContactNumber;
            userRecord.Email = request.Email;

            var duplicate = await UserService.GetDuplicateAsync(userRecord);
            var duplicateEmail = await UserService.GetDuplicateEmailAsync(userRecord);

            if (duplicate is not null && duplicate.UserId != userId)
                return Conflict(ResponseMessage.Duplicate);

            if (duplicateEmail != null && duplicateEmail.UserId != userId)
                return Conflict(ResponseMessage.Duplicate);

            await UserService.UpdateAsync(userRecord);
            if (request.UserRoles is not null && request.UserRoles.Any())
            {
                var currentRoles = await UserRoleService.GetByUserAsync(userId);
                if(currentRoles.Any())
                {
                    foreach (var role in currentRoles)
                    {
                        await UserRoleService.DeleteAsync(role.UserRoleId);
                    }
                }
               
                foreach (var userRole in request.UserRoles)
                {
                    var userRoleById = await UserRoleService.GetByUserIdAndRoleIdAsync(userRole.UserId, userRole.RoleId);
                    if (userRoleById is not null)
                    {
                        await UserRoleService.UnDeleteAsync(userRoleById.UserRoleId);
                    } else
                    {
                        await UserRoleService.InsertAsync(userRole);
                    }
                }
            }

            return Ok();
        }

        [Authorize]
        [HttpPut("Approved/{userId}")]
        public async Task<IActionResult> ApprovedAsync(int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            var user = await UserService.GetAsync(userId);
            
            if(user is not null)
            {
                await UserService.ApprovedAsync(userId);

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
                    var emailTemplate = await GetEmailTemplate(AppSettings.WebUrl + "html/templates/NewUserTemplate.html");
                    await MailService.SendForgotPasswordEmailAsync(user.Email!, user.Username!, randomPassword, emailTemplate);
                }
                catch (Exception ex)
                {
                    return BadRequest(ex.Message);
                }
            }
            

            return Ok();
        }


        [Authorize]
        [HttpPut("{userId}/Status/{status}")]
        public async Task<IActionResult> UpdateStatusAsync(int userId, bool status)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");
            var user = await UserService.GetAsync(userId);

            if(user is null)
                return BadRequest("Bad Request.");

            user.Active = status;

            await UserService.UpdateAsync(user);

            if(user.Active)
            {
                await SendNewPassword(user);
            }

            return Ok();
        }
        [Authorize]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteAsync(int userId)
        {
            if (!ModelState.IsValid)
                return BadRequest("Bad Request.");

            var deleted = await UserService.DeleteAsync(userId);
            if (!deleted)
                return NotFound(ResponseMessage.NotFound);

            return Ok();
        }

        [HttpPut("Reset")]
        [Authorize]
        public async Task<IActionResult> ResetPassword(ResetPasswordRequestModel request)
        {
            var user = await UserService.GetAsync(request.UserId);

            if (user == null)
                return NotFound(ResponseMessage.NotFound);

            var valid = Hash.Validate(request.CurrentPassword!, user.Salt!, user.Password!);

            if (!valid)
                return Conflict();

            var salt = Salt.Create();

            user.Password = Hash.Create(request.Password!, salt);
            user.Salt = salt;
            user.IsPasswordChanged = false;
            user.Locked = false;
            user.LoginAttempts = 0;

            await UserService.ResetPassword(user);

            return Ok();
        }
    }
}
