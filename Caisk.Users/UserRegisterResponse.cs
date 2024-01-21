namespace Caisk.Users;

public record UserRegisterResponse(ValidationResult Result, UserSummary User, string Totp);