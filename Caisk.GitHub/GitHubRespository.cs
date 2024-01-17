using System.Collections;
using Caisk.Objects;

namespace Caisk.GitHub;

public class GitHubRepositoryProfile : ObjectProfile
{
    public string? OrganizationName { get; set; }
    public string? RepositoryName { get; set; }
    public string? Token { get; set; }
    public List<GitHubWorkflow> Workflows { get; init; } = [];
}

public class GitHubWorkflow
{
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
}