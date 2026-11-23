using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Utility.Interface;

namespace SCICHRPortal.Service.Interfaces;

public interface IUserAuthenticationService : IScopedService
{
    Task<UserAuthenticationResult> LoginAsync(string? email, string? password, CancellationToken cancellationToken);
    Task<UserAuthenticationResult> ForgotPasswordAsync(string? email, CancellationToken cancellationToken);
}
