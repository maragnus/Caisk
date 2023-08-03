namespace Caisk.Managers.Kubernetes;

public class KubernetesProfile : ObjectProfile
{
    public string? Configuration { get; set; }

    public k8s.Kubernetes GetKubernetes()
    {
        PropertyRequired(Configuration);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(Configuration));
        var config = KubernetesClientConfiguration.BuildConfigFromConfigFile(stream);
        return new k8s.Kubernetes(config);
    }
}