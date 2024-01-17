using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Caisk.Applications;
using Caisk.Data;
using Caisk.Docker;
using Caisk.Docker.DockerCompose;
using Caisk.GitHub;
using Caisk.SecureShells;
using Service = Caisk.Applications.Service;

namespace Caisk.Deploy;

public class DeploymentManager(IDataContext dataContext, SecureShellManager sshManager)
{
    private static readonly Regex InvalidFileNameRegex = new(@"[<>&/\\]");

    public async Task DeployApplication(string appName, string environmentName)
    {
        var (app, env) = await GetApplicationEnvironment(appName, environmentName);
        var ssh = await sshManager.CreateSecureShell(env.SecureShellName ??
                                                     throw new Exception("Secure Shell is required"));
        var mongo = await dataContext.MongoDatabaseStore.Get(env.MongoDatabaseName);

        var root = $"~/.caisk/{app.Name}/{env.Name}";
        var composeFile = $"{root}/docker-compose.yaml";
        var yaml = Convert.ToBase64String(Encoding.UTF8.GetBytes(env.DockerCompose ?? ""));

        await using var sh = await ssh.ConnectAsync();
        await sh.ExecuteAsync($"mkdir -p {root}");
        await sh.ExecuteAsync($"echo '{yaml}' | base64 --decode > {composeFile}");

        foreach (var file in env.Files)
        {
            if (InvalidFileNameRegex.IsMatch(file.FileName))
                throw new Exception($"Invalid filename: {file.FileName}");
            var dest = $"{root}/{file.FileName}";
            var contents = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.Contents ?? ""));
            if (file.Base64)
                await sh.ExecuteAsync($"echo '{contents}' | base64 --decode | base64 --decode > {dest}");
            else
                await sh.ExecuteAsync($"echo '{contents}' | base64 --decode > {dest}");
        }

        await sh.ExecuteAsync($"echo 'docker-compose -p {app.Name}-{env.Name} -f {composeFile} up' > ~/test");
    }

    public static void UpdateServices(ApplicationEnvironmentProfile env)
    {
        if (string.IsNullOrWhiteSpace(env.DockerCompose))
        {
            env.Services.Clear();
            return;
        }

        var services = new List<Service>();
        var compose = DockerCompose.ParseYaml(env.DockerCompose);
        foreach (var service in compose.Services)
        {
            var newService = env.Services.FirstOrDefault(s => s.Name == service.Key)
                             ?? new Service() { Name = service.Key };
            newService.Image = DockerImage.Parse(service.Value.Image);
            services.Add(newService);
        }

        env.Services.Clear();
        env.Services.AddRange(services);
    }


    private async Task<(ApplicationProfile app, ApplicationEnvironmentProfile env)> GetApplicationEnvironment(
        string appName, string environmentName)
    {
        var app = await dataContext.ApplicationStore.Get(appName)
                  ?? throw new Exception("Application not found");
        var env = await dataContext.ApplicationEnvironmentStore.Get(environmentName, appName)
                  ?? throw new Exception("Application Environment not found");
        return (app, env);
    }

    public static async Task UpdateGitHubWorkflows(GitHubRepositoryProfile profile)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new("request", "1.0"));
        client.DefaultRequestHeaders.Authorization = new("Bearer", profile.Token);
        var body = await client.GetStringAsync(
            $"https://api.github.com/repos/{profile.OrganizationName}/{profile.RepositoryName}/actions/workflows");
        var json = JsonDocument.Parse(body);
        var workflows = json.RootElement.GetProperty("workflows").EnumerateArray();
        profile.Workflows.Clear();
        foreach (var workflow in workflows)
            profile.Workflows.Add(new GitHubWorkflow()
            {
                Id = workflow.GetProperty("id").GetInt32().ToString() ?? "",
                Name = workflow.GetProperty("name").GetString() ?? "",
            });
    }
}