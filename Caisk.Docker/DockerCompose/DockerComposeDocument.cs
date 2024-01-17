using JetBrains.Annotations;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization.NamingConventions;

namespace Caisk.Docker.DockerCompose;

[PublicAPI]
public class DockerCompose
{
    public Version Version { get; set; } = new Version(0, 0);
    public Dictionary<string, Service> Services { get; init; } = [];
    public Dictionary<string, Network> Networks { get; init; } = [];
    public Dictionary<string, Volume> Volumes { get; init; } = [];

    public static DockerCompose ParseYaml(string yaml)
    {
        if (string.IsNullOrWhiteSpace(yaml)) return new DockerCompose();

        var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();
        return deserializer.Deserialize<DockerCompose>(yaml);
    }

    public static string UpdateServiceImage(string yaml, string serviceName, DockerImage image)
    {
        var input = new StringReader(yaml);
        var yamlStream = new YamlStream();
        yamlStream.Load(input);

        foreach (var document in yamlStream.Documents)
        {
            var node = (YamlScalarNode)document.RootNode["services"][serviceName]["image"];
            node.Value = image.ToString();
        }

        using var writer = new StringWriter();
        yamlStream.Save(writer);
        return writer.ToString();
    }
}

public class Service
{
    public string? Image { get; set; }
    public List<string> Ports { get; set; } = [];
    public List<string> Networks { get; set; } = [];
}

public class Network
{
}

public class Volume
{
    public bool External { get; set; }
}