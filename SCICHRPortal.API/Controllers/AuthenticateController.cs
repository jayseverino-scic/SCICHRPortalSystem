using Microsoft.AspNetCore.Mvc;
using SCICHRPortal.API.Models.RequestModels.Authenticate;
using SCICHRPortal.API.Models.RequestModels.Authenticated.Users;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Service.Interfaces;

namespace SCICHRPortal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private IUserAuthenticationService Authentication { get; }
        public AuthenticateController(IUserAuthenticationService authentication) { Authentication = authentication; }

        [HttpPost("User")]
        public async Task<IActionResult> Authenticate(LoginRequestModel request)
        {
            var result = await Authentication.LoginAsync(request.Email, request.Password, HttpContext.RequestAborted);
            return result.Status switch
            {
                UserAuthenticationStatus.Invalid => BadRequest(result.Message),
                UserAuthenticationStatus.Unauthorized => Unauthorized(result.Message),
                UserAuthenticationStatus.Forbidden => Forbid(),
                UserAuthenticationStatus.Locked => Conflict(result.Message),
                UserAuthenticationStatus.Unavailable => StatusCode(503, result.Message),
                _ => Ok(new { accessToken = new { result.JsonWebToken, result.RefreshToken }, result.IsPasswordChanged })
            };
        }

        [HttpPost("User/ForgotPassword")]
        public async Task<IActionResult> ResetPassword(ForgotPasswordRequestModel request)
        {
            var result = await Authentication.ForgotPasswordAsync(request.Email, HttpContext.RequestAborted);
            return result.Status switch
            {
                UserAuthenticationStatus.NotFound => NotFound(result.Message),
                UserAuthenticationStatus.Invalid or UserAuthenticationStatus.DeliveryFailed => BadRequest(result.Message),
                UserAuthenticationStatus.Unavailable => StatusCode(503, result.Message),
                _ => Ok()
            };
        }
    }
}
