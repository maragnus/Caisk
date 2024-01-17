using System.Diagnostics;
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

public record DeployStatusUpdate(string StatusText, bool IsIndeterminate = true, int Value = 0, int Max = 100)
{
    public DeployStatusUpdate(string statusText) : this(statusText, true)
    {
    }

    public DeployStatusUpdate(string statusText, int value) : this(statusText, false, value)
    {
    }

    public DeployStatusUpdate(string statusText, int value, int max) : this(statusText, false, value, max)
    {
    }
}

public class DeploymentManager(IDataContext dataContext, SecureShellManager sshManager)
{
    private const int GithubRepoQueryIntervalSeconds = 15;
    private static readonly Regex InvalidFileNameRegex = new(@"[<>&/\\]");

    private static readonly string[] RunningStatuses =
        { "in_progress", "queued", "requested", "waiting", "pending" };

    public async IAsyncEnumerable<DeployStatusUpdate> DeployApplication(string appName, string environmentName,
        CancellationToken cancellationToken)
    {
        yield return new DeployStatusUpdate("Loading deployment configuration...");
        var (app, env) = await GetApplicationEnvironment(appName, environmentName);
        var secureShellName = env.SecureShellName ?? throw new Exception("Secure Shell is required for deployment");
        var ssh = await sshManager.CreateSecureShell(secureShellName);

        var progress = 0;
        var max = env.Files.Count + env.Services.Count + 4;

        cancellationToken.ThrowIfCancellationRequested();
        yield return new DeployStatusUpdate("Verifying Mongo Database...", ++progress, max);
        var mongo = await dataContext.MongoDatabaseStore.Get(env.MongoDatabaseName);

        foreach (var service in env.Services)
        {
            progress++;

            if (string.IsNullOrEmpty(service.RepositoryName) || string.IsNullOrEmpty(service.ActionId) ||
                service.Image is null)
                continue;

            var tag = service.Image.Value.Tag!;
            var timer = Stopwatch.StartNew();
            int elapsed() => (int)timer!.Elapsed.TotalSeconds;
            var name = $"GitHub Workflow {service.RepositoryName} #{tag}";

            cancellationToken.ThrowIfCancellationRequested();
            yield return new DeployStatusUpdate($"Checking {name}...", progress, max);

            var gitHubRepo = await dataContext.GitHubRepositoryStore.Require(service.RepositoryName);
            var statuses = await GetGitHubActionRuns(gitHubRepo, service.ActionId, cancellationToken);
            var status = statuses.FirstOrDefault(r => r.RunId.ToString() == tag);
            var runId = status.Id;

            while (!status.IsSuccessful && !cancellationToken.IsCancellationRequested)
            {
                for (var i = 0; i < GithubRepoQueryIntervalSeconds; i += 1)
                {
                    yield return new DeployStatusUpdate($"Waiting for {name}: {status.Status} ({elapsed()}s)...",
                        progress, max);
                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
                }

                yield return new DeployStatusUpdate($"Updating {name}... ({elapsed()}s)...", progress, max);
                status = await GetGitHubActionRun(gitHubRepo, runId, cancellationToken);
                if (status.IsError) throw new Exception($"{name} failed with status {status.Status}");
                if (status.IsSuccessful) break;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        yield return new DeployStatusUpdate("Uploading docker-compose to server...", ++progress, max);
        var root = $"~/.caisk/{app.Name}/{env.Name}";
        var composeFile = $"{root}/docker-compose.yaml";
        var yaml = Convert.ToBase64String(Encoding.UTF8.GetBytes(env.DockerCompose ?? ""));

        await using var sh = await ssh.ConnectAsync();
        await sh.ExecuteAsync($"mkdir -p {root}", cancellationToken);
        await sh.ExecuteAsync($"echo '{yaml}' | base64 --decode > {composeFile}", cancellationToken);

        foreach (var file in env.Files)
        {
            yield return new DeployStatusUpdate($"Uploading {file.FileName} to server...", ++progress, max);
            if (InvalidFileNameRegex.IsMatch(file.FileName))
                throw new Exception($"Invalid filename: {file.FileName}");
            var dest = $"{root}/{file.FileName}";
            var contents = Convert.ToBase64String(Encoding.UTF8.GetBytes(file.Contents ?? ""));
            if (file.Base64)
                await sh.ExecuteAsync($"echo '{contents}' | base64 --decode | base64 --decode > {dest}",
                    cancellationToken);
            else
                await sh.ExecuteAsync($"echo '{contents}' | base64 --decode > {dest}", cancellationToken);
        }

        yield return new DeployStatusUpdate("Activating deployment...", ++progress, max);
        await sh.ExecuteAsync($"cd {root} && docker-compose -p {app.Name}-{env.Name} up -d", cancellationToken);

        yield return new DeployStatusUpdate("Deployment completed", max, max);
    }

    public static void ApplyServices(ApplicationEnvironmentProfile env)
    {
        if (string.IsNullOrWhiteSpace(env.DockerCompose))
        {
            env.Services.Clear();
            return;
        }

        foreach (var service in env.Services)
        {
            if (string.IsNullOrEmpty(service.Name) || !service.Image.HasValue) continue;
            env.DockerCompose = DockerCompose.UpdateServiceImage(env.DockerCompose, service.Name, service.Image.Value);
        }
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

    public static async Task UpdateGitHubWorkflows(GitHubRepositoryProfile profile, CancellationToken cancellationToken)
    {
        var client = CreateGitHubHttpClient(profile);
        var endpoint =
            $"https://api.github.com/repos/{profile.OrganizationName}/{profile.RepositoryName}/actions/workflows";
        var body = await client.GetStringAsync(endpoint, cancellationToken);
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

    public static async Task<GitHubRunStatus[]> GetGitHubActionRuns(GitHubRepositoryProfile profile, string actionId,
        CancellationToken cancellationToken = default)
    {
        var client = CreateGitHubHttpClient(profile);
        var endpoint =
            $"https://api.github.com/repos/{profile.OrganizationName}/{profile.RepositoryName}/actions/workflows/{actionId}/runs";
        var body = await client.GetStringAsync(endpoint, cancellationToken);
        var json = JsonDocument.Parse(body);
        var runs = json.RootElement.GetProperty("workflow_runs")
            .EnumerateArray().OrderByDescending(run => run.GetProperty("run_number").GetInt32())
            .ToList();
        var results = new List<GitHubRunStatus>(runs.Count);
        foreach (var run in runs)
        {
            var status = run.GetProperty("status").GetString() ?? "<unknown>";
            var runNumber = run.GetProperty("run_number").GetInt32();
            var id = run.GetProperty("id").GetInt64();
            results.Add(new GitHubRunStatus(status == "completed", RunningStatuses.Contains(status), id, runNumber,
                status));
        }

        return results.ToArray();
    }

    private static async Task<GitHubRunStatus> GetGitHubActionRun(GitHubRepositoryProfile profile, long runId,
        CancellationToken cancellationToken)
    {
        var client = CreateGitHubHttpClient(profile);
        var endpoint =
            $"https://api.github.com/repos/{profile.OrganizationName}/{profile.RepositoryName}/actions/runs/{runId}";
        var body = await client.GetStringAsync(endpoint, cancellationToken);
        var json = JsonDocument.Parse(body);
        var run = json.RootElement;
        var status = run.GetProperty("status").GetString() ?? "<unknown>";
        var id = run.GetProperty("id").GetInt64();
        var runNumber = run.GetProperty("run_number").GetInt32();
        return new GitHubRunStatus(status == "completed", RunningStatuses.Contains(status), id, runNumber, status);
    }

    private static HttpClient CreateGitHubHttpClient(GitHubRepositoryProfile profile)
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.UserAgent.Add(new("request", "1.0"));
        client.DefaultRequestHeaders.Authorization = new("Bearer", profile.Token);
        return client;
    }
}

public readonly struct GitHubRunStatus(bool isSuccessful, bool isRunning, long id, int? runId, string status)
{
    public long Id { get; } = id;
    public int? RunId { get; } = runId;
    public string Status { get; } = status;
    public bool IsError { get; } = !isRunning && !isSuccessful;
    public bool IsSuccessful { get; } = isSuccessful;
    public bool IsRunning { get; } = isRunning;

    public override string ToString() =>
        $"RunId={RunId} Status={Status} IsSuccessful={IsSuccessful} IsRunning={IsRunning} IsError={IsError}";
}