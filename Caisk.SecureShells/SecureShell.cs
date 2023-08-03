namespace Caisk.SecureShells;

[PublicAPI]
public class SecureShell
{
    private readonly SecureShellManager _secureShellManager;
    private readonly SecureShellProfile _profile;

    public SecureShell(SecureShellProfile profile, SecureShellManager secureShellManager)
    {
        _profile = profile;
        _secureShellManager = secureShellManager;
    }

    public async Task<SshClientAsync> ConnectAsync()
    {
        var methods = new List<AuthenticationMethod>();
        
        if (_profile.Password != null)
            methods.Add(
                new PasswordAuthenticationMethod(_profile.UserName, _profile.Password));

        if (_profile.KeyPairNames?.Length > 0)
        {
            var keys = await _secureShellManager.PrivateKeyStore.Get(_profile.KeyPairNames);
            var privateKeys = keys.Select(x => x.ToPrivateKeyFile()).ToArray();
            methods.Add(
                new PrivateKeyAuthenticationMethod(_profile.UserName,privateKeys));
        }
        
        var connectionInfo = new ConnectionInfo(_profile.Host, _profile.UserName, methods.ToArray());

        var client = new SshClientAsync(connectionInfo);
        await client.ConnectAsync();
        return client;
    }

    public async Task InstallKeyPair(params string[] keyPairNames)
    {
        var keys = await _secureShellManager.PrivateKeyStore.Get(keyPairNames);
        var client = await ConnectAsync();
        try
        {
            var result = await client.SendCommandAsync("cat ~/.ssh/authorized_keys");
            var registeredKeys = result.Result.Split(new char[] { '\r', '\n' })
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToHashSet();

            foreach (var key in keys)
            {
                var authorizedKey = key.PublicKey;
                if (registeredKeys.Contains(authorizedKey))
                    continue;
                
                var mkdirResult = await client.SendCommandAsync("mkdir -p ~/.ssh");
                if (mkdirResult.ExitCode != 0)
                    throw new Exception($"Failed to create directory ~/.ssh for key {key.Name}: {mkdirResult.Error}");

                var appendResult = await client.SendCommandAsync($@"echo ""{authorizedKey}"" >> ~/.ssh/authorized_keys");
                if (appendResult.ExitCode != 0)
                    throw new Exception($"Failed to append key {key.Name}: {appendResult.Error}");
            }
        }
        finally
        {
            await client.DisconnectAsync();
        }
    }
}