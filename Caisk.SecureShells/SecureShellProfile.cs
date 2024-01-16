namespace Caisk.SecureShells;

public class SecureShellProfile : ObjectProfile
{
    public string? MachineInfo { get; set; }
    public string? HostName { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? RootPassword { get; set; }
    public string? KeyPairName { get; set; }
    public string? Fingerprint { get; set; }

    public SecureShellProfile ToSummary() => new()
    {
        Id = Id,
        Name = Name,
        HostName = HostName,
        UserName = UserName
    };
}