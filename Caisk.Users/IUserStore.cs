using Caisk.Objects;

namespace Caisk.Users;

public interface IUserStore : IObjectProfileStore<UserProfile>
{
    Task<UserValidationResponse> ValidateSessionToken(string sessionToken);
    Task<UserValidationResponse> Login(string userName, string password, string? totpCode);
    Task<UserRegisterResponse> Register(string userName, string password);
    Task<byte[]> TotpPng(string userName, string totpSecret);
}