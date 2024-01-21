namespace Caisk.Users;

public record UserValidationResponse(ValidationResult Result, UserSummary? User);