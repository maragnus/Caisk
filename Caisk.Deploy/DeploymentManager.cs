using System.Text;
using Caisk.Data;
using Caisk.SecureShells;

namespace Caisk.Deploy;

public class DeploymentManager(IDataContext dataContext, SecureShellManager sshManager)
{
    public async Task DeployApplication(string appName, string environmentName)
    {
        var app = await dataContext.ApplicationStore.Get(appName) 
                  ?? throw new Exception("Application not found");
        var env = await dataContext.ApplicationEnvironmentStore.Get(environmentName, appName) 
                  ?? throw new Exception("Application Environment not found");
        var ssh = await sshManager.CreateSecureShell(env.SecureShellName ?? throw new Exception("Secure Shell is required"));
        var mongo = await dataContext.MongoDatabaseStore.Get(env.MongoDatabaseName);

        var root = $"~/.caisk/{app.Name}/{env.Name}";
        var composeFile = $"{root}/docker-compose.yaml";
        var yaml = Convert.ToBase64String(Encoding.UTF8.GetBytes(env.DockerCompose ?? ""));
        
        await using var sh = await ssh.ConnectAsync();
        await sh.ExecuteAsync($"mkdir -p {root}");
        await sh.ExecuteAsync($"echo '{yaml}' | base64 --decode > {composeFile}");
        await sh.ExecuteAsync($"echo 'docker-compose -p {app.Name}-{env.Name} -f {composeFile} up' > ~/test");
    }
}