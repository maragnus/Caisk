using System.Diagnostics;
using Caisk.Objects;
using Caisk.Users;

namespace Caisk.Data.LiteDb;

internal class UserStore(ILiteCollection<UserProfile> collection)
    : BaseStore<UserProfile>(collection), IUserStore
{
    public TimeSpan MinTime { get; } = TimeSpan.FromSeconds(2);

    public override string Name => "User";

    public Task<UserValidationResponse> ValidateSessionToken(string sessionToken)
    {
        var user = Collection.FindOne(u => u.SessionTokens.Contains(sessionToken));
        if (user is null)
            return Task.FromResult(new UserValidationResponse(ValidationResult.Invalid, null));
        return Task.FromResult(new UserValidationResponse(ValidationResult.Success, new UserSummary
        {
            Id = user.Id,
            Name = user.Name,
            IsVerified = user.IsVerified,
        }));
    }

    public async Task<UserValidationResponse> Login(string userName, string password, string? totp)
    {
        var start = Stopwatch.StartNew();
        var result = LoginInternal(userName, password, totp);

        var delay = MinTime - start.Elapsed;
        if (delay > TimeSpan.Zero) await Task.Delay(delay);

        return result;
    }

    public async Task<UserRegisterResponse> Register(string userName, string password)
    {
        if (await TryGet(userName) is not null)
            throw new ProfileStoreException("Name already exists");

        var user = await Create(userName);
        user.PasswordHash = UserUtility.HashPassword(password, user.Id);
        user.TotpSecret = UserUtility.GenerateTotpSecret();
        await Store(user);
        return new UserRegisterResponse(ValidationResult.Success, Summarize(user), user.TotpSecret);
    }

    public Task<byte[]> TotpPng(string totpSecret, string userName) =>
        Task.FromResult(UserUtility.TotpQrCodePng(userName, totpSecret));

    private UserValidationResponse LoginInternal(string userName, string password, string? totp)
    {
        var salt = Collection.FindOne(u => u.Name == userName)?.Id;
        if (string.IsNullOrWhiteSpace(salt)) return new UserValidationResponse(ValidationResult.Invalid, null);

        var pw = UserUtility.HashPassword(password, salt);
        var user = Collection.FindOne(u => u.Name == userName && u.PasswordHash == pw);

        var requiresTotp = !string.IsNullOrWhiteSpace(user.TotpSecret);
        var providedTotp = !string.IsNullOrWhiteSpace(totp);

        if (requiresTotp && !providedTotp)
            if (string.IsNullOrWhiteSpace(salt))
                return new UserValidationResponse(ValidationResult.Invalid, null);

        if (requiresTotp && providedTotp && !UserUtility.VerifyTotp(DateTime.Now, user.TotpSecret!, totp!))
            if (string.IsNullOrWhiteSpace(salt))
                return new UserValidationResponse(ValidationResult.Invalid, null);

        return new UserValidationResponse(ValidationResult.Success, Summarize(user));
    }

    private static UserSummary Summarize(UserProfile user) =>
        new()
        {
            Id = user.Id,
            Name = user.Name,
            IsVerified = user.IsVerified
        };
}