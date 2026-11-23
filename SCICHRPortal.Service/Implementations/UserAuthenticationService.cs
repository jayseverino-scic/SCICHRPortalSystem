using System.Security.Cryptography;
using Microsoft.Extensions.Logging;
using SCICHRPortal.Data.DTOs;
using SCICHRPortal.Data.Entities;
using SCICHRPortal.Repository.Interfaces;
using SCICHRPortal.Service.Interfaces;
using SCICHRPortal.Utility.Constants;
using SCICHRPortal.Utility.Cryptography;

namespace SCICHRPortal.Service.Implementations;

public sealed class UserAuthenticationService : IUserAuthenticationService
{
    private IUserManagementRepository Users { get; }
    private IAccessTokenService Tokens { get; }
    private IUserCredentialDelivery Delivery { get; }
    private ILogger<UserAuthenticationService> Logger { get; }
    public UserAuthenticationService(IUserManagementRepository users, IAccessTokenService tokens, IUserCredentialDelivery delivery,
        ILogger<UserAuthenticationService> logger)
    {
        Users = users; Tokens = tokens; Delivery = delivery; Logger = logger;
    }

    public async Task<UserAuthenticationResult> LoginAsync(string? email, string? password, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password)) return new(UserAuthenticationStatus.Invalid, "Email and password are required.");
        var normalizedEmail = email.Trim().ToLowerInvariant();
        return await ExecuteAsync(async () =>
        {
            var account = await Users.GetAccountByEmailAsync(normalizedEmail, cancellationToken);
            if (account == null || account.Deleted) return new(UserAuthenticationStatus.Unauthorized, ResponseMessage.InvalidCredentials);
            if (!account.IsApproved || !account.Active) return new(UserAuthenticationStatus.Forbidden);
            if (!Hash.Validate(password, account.Salt!, account.Password!) || account.Locked)
            {
                account.LoginAttempts++;
                // Preserve the seeded administrator exemption and existing ten-attempt policy.
                var exceeded = account.LoginAttempts >= 10 && account.UserId != 1;
                if (exceeded) account.Locked = true;
                Stamp(account, "Authentication");
                await Users.SaveAccountAsync(account, "Authentication", cancellationToken);
                return exceeded ? new(UserAuthenticationStatus.Locked, ResponseMessage.LoginAttemptsExceeded)
                    : new(UserAuthenticationStatus.Unauthorized, ResponseMessage.InvalidCredentials);
            }
            var roles = (account.UserRoles ?? []).Where(link => !link.Deleted && link.Role is { Deleted: false }).Select(link => link.Role!);
            var token = await Tokens.Generate(account, roles);
            account.LoginAttempts = 0;
            Stamp(account, "Authentication");
            await Users.SaveAccountAsync(account, "Authentication", cancellationToken);
            return new(UserAuthenticationStatus.Success, JsonWebToken: token.JsonWebToken, RefreshToken: token.RefreshToken, IsPasswordChanged: account.IsPasswordChanged);
        }, cancellationToken);
    }

    public async Task<UserAuthenticationResult> ForgotPasswordAsync(string? email, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(email)) return new(UserAuthenticationStatus.Invalid, "Email is required.");
        var normalizedEmail = email.Trim().ToLowerInvariant();
        var password = "S" + RandomNumberGenerator.GetHexString(20) + "9";
        var salt = Salt.Create();
        User? recipient = null;
        var saved = await ExecuteAsync(async () =>
        {
            recipient = null;
            var account = await Users.GetAccountByEmailAsync(normalizedEmail, cancellationToken);
            if (account == null || account.Deleted) return new(UserAuthenticationStatus.NotFound, ResponseMessage.NotFound);
            account.Password = Hash.Create(password, salt); account.Salt = salt;
            account.IsPasswordChanged = false; account.Locked = false; account.LoginAttempts = 0;
            Stamp(account, "Password recovery");
            await Users.SaveAccountAsync(account, "Password recovery", cancellationToken);
            recipient = account;
            return new(UserAuthenticationStatus.Success);
        }, cancellationToken);
        if (saved.Status != UserAuthenticationStatus.Success) return saved;
        try
        {
            await Delivery.SendAsync(recipient!.Email!, $"{recipient.LastName}, {recipient.FirstName}", password, cancellationToken);
            return saved;
        }
        catch (Exception exception)
        {
            Logger.LogWarning(exception, "User {UserId} password recovery was saved but delivery failed.", recipient!.UserId);
            return new(UserAuthenticationStatus.DeliveryFailed, "The password was reset, but its email could not be delivered. Contact your Administrator.");
        }
    }

    private async Task<UserAuthenticationResult> ExecuteAsync(Func<Task<UserAuthenticationResult>> operation, CancellationToken cancellationToken)
    {
        // Retry only rolled-back authentication database work; mail delivery remains outside this callback.
        for (var retry = 0; ; retry++)
        {
            try
            {
                UserAuthenticationResult? result = null;
                await Users.ExecuteWriteAsync(async () => { result = await operation(); }, cancellationToken);
                return result ?? throw new InvalidOperationException("Authentication did not produce a result.");
            }
            catch (UserPersistenceConflictException) when (retry < 8)
            {
                await Task.Delay(Random.Shared.Next(5, 20), cancellationToken);
            }
            catch (UserPersistenceConflictException)
            {
                return new(UserAuthenticationStatus.Unavailable, "The account changed while processing your request. Please try again.");
            }
        }
    }
    private static void Stamp(User account, string actor)
    {
        account.UpdatedAt = DateTime.UtcNow;
        account.UpdatedBy = actor;
    }
}
