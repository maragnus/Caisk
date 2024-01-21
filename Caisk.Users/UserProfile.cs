using Caisk.Objects;

namespace Caisk.Users;

public class UserProfile : ObjectProfile
{
    public string? TotpSecret { get; set; }
    public string? PasswordHash { get; set; }
    public string[] SessionTokens { get; init; } = [];
    public bool IsVerified { get; set; }
}