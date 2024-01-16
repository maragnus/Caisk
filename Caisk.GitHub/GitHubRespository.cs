using Caisk.Objects;

namespace Caisk.GitHub;

public class GitHubRepositoryProfile : ObjectProfile
{
    public string? OrganizationName { get; set; }
    public string? RepositoryName { get; set; }
    public string? Token { get; set; }
}