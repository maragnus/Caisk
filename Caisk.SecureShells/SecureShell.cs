namespace Caisk.SecureShells;

[PublicAPI]
public class SecureShell(SecureShellProfile profile, SecureShellManager secureShellManager)
{
    public SecureShellProfile Profile => profile;

    public async Task<SshClientAsync> ConnectAsync()
    {
        var methods = new List<AuthenticationMethod>();
        
        if (profile.Password != null)
            methods.Add(
                new PasswordAuthenticationMethod(profile.UserName, profile.Password));

        if (!string.IsNullOrWhiteSpace(profile.KeyPairName))
        {
            var key = await secureShellManager.PrivateKeyStore.Get(profile.KeyPairName)
                ?? throw new KeyNotFoundException($"Private Key \"{profile.KeyPairName}\" was not found");
            methods.Add(new PrivateKeyAuthenticationMethod(profile.UserName, key.ToPrivateKey()));
        }
        
        var connectionInfo = new ConnectionInfo(profile.HostName, profile.UserName, methods.ToArray());

        var client = new SshClientAsync(connectionInfo);
        await client.ConnectAsync();
        return client;
    }

    public async Task InstallKeyPair(params string[] keyPairNames)
    {
        var keys = await secureShellManager.PrivateKeyStore.Get(keyPairNames);
        var client = await ConnectAsync();
        try
        {
            var result = await client.SendCommandAsync("cat ~/.ssh/authorized_keys");
            var registeredKeys = result.Result.Split(['\r', '\n'])
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Select(line => line.Trim())
                .ToHashSet();

            foreach (var key in keys)
            {
                var authorizedKey = key.PublicKey;
                if (authorizedKey == null || !registeredKeys.Contains(authorizedKey))
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