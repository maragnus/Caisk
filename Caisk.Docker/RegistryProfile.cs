using Caisk.Objects;

namespace Caisk.Docker;

public class RegistryProfile : ObjectProfile
{
    public string? HostName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? BearerToken { get; set; }
    public RegistryAuthenticationType AuthenticationType { get; set; } = RegistryAuthenticationType.Anonymous;
}