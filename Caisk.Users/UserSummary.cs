namespace Caisk.Users;

public class UserSummary
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public bool IsVerified { get; init; }
}