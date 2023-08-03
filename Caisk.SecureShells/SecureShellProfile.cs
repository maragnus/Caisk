namespace Caisk.SecureShells;

public class SecureShellProfile : ObjectProfile
{
    public string? MachineInfo { get; set; }
    public string? Host { get; set; }
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? RootPassword { get; set; }
    public string[]? KeyPairNames { get; set; }
    public byte[]? Fingerprint { get; set; }

    public SecureShellProfile ToSummary() => new()
    {
        Id = Id,
        Name = Name,
        Host = Host,
        UserName = UserName
    };
}