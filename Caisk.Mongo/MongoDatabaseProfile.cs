using Caisk.Objects;

namespace Caisk.Managers.Mongo;

public class MongoDatabaseProfile : ObjectProfile
{
    public string? HostName { get; set; }
    public int? Port { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Database { get; set; }
    public string? AuthenticationDatabase { get; set; }
    public string? SecureShellName { get; set; }
    public int? TunnelLocalPort { get; set; }
}