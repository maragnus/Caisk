using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Utilities;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace Caisk.SecureShells;

[PublicAPI]
public class PrivateKeyProfile : ObjectProfile
{
    public PrivateKeyType? Type { get; set; }
    public string? PrivateKey { get; set; }
    public string? PublicKey { get; set; }
    public DateTime? Generated { get; set; }

    private const int DsaCertainty = 100;

    public static PrivateKeyProfile Generate(string name, PrivateKeyType type, int? bits = null)
    {
        ValidateName(name);
        
        var keyPair = type switch
        {
            PrivateKeyType.Rsa => GenerateRsaPrivate(bits ??
                                                     throw new ArgumentNullException(nameof(bits),
                                                         "RSA key requires bits argument")),
            PrivateKeyType.Dsa => GenerateDsaPrivate(bits ??
                                                     throw new ArgumentNullException(nameof(bits),
                                                         "DSA key requires bits argument")),
            PrivateKeyType.Ecdsa => GenerateEcdsaPrivate(bits ??
                                                         throw new ArgumentNullException(nameof(bits),
                                                             "ECDSA key requires bits argument")),
            PrivateKeyType.Ed22519 => GenerateEd22519Private(),
            _ => throw new ArgumentOutOfRangeException(nameof(type), $"{type} is not supported")
        };

        return CreatePrivateKeyProfile(name, type, keyPair);
    }

    private static AsymmetricCipherKeyPair GenerateRsaPrivate(int bits)
    {
        var generator = new RsaKeyPairGenerator();
        var parameters = new KeyGenerationParameters(new SecureRandom(), bits);
        generator.Init(parameters);
        return generator.GenerateKeyPair();
    }

    private static AsymmetricCipherKeyPair GenerateDsaPrivate(int bits)
    {
        var random = new SecureRandom();
        var parametersGenerator = new DsaParametersGenerator();
        parametersGenerator.Init(bits, DsaCertainty, random);
        var generator = new DsaKeyPairGenerator();
        var parameters = new DsaKeyGenerationParameters(random, parametersGenerator.GenerateParameters());
        generator.Init(parameters);
        return generator.GenerateKeyPair();
    }

    private static AsymmetricCipherKeyPair GenerateEcdsaPrivate(int bits)
    {
        if (bits != 256 && bits != 384 && bits != 512)
            throw new ArgumentOutOfRangeException(nameof(bits), "ECDSA bits argument must be 256, 384, or 512");
        throw new InvalidOperationException("Generation of ECDSA Key Pair is not supported");
    }

    private static AsymmetricCipherKeyPair GenerateEd22519Private()
    {
        var generator = new Ed25519KeyPairGenerator();
        var parameters = new Ed25519KeyGenerationParameters(new SecureRandom());
        generator.Init(parameters);
        return generator.GenerateKeyPair();
    }

    private static PrivateKeyProfile CreatePrivateKeyProfile(string name, PrivateKeyType type,
        AsymmetricCipherKeyPair keyPair)
    {
        var publicKey = GeneratePrivateKey(keyPair, type);
        var privateKey = GeneratePublicKey(name, keyPair, type);

        var now = DateTime.Now;
        return new PrivateKeyProfile
        {
            Name = name,
            Created = now,
            Updated = now,
            Generated = now,
            PrivateKey = publicKey,
            PublicKey = privateKey,
            Type = type
        };
    }

    private static string GeneratePrivateKey(AsymmetricCipherKeyPair keyPair, PrivateKeyType type)
    {
        var typeName = type switch
        {
            PrivateKeyType.Rsa => "RSA",
            PrivateKeyType.Dsa => "DSA",
            PrivateKeyType.Ecdsa or PrivateKeyType.Ed22519 => "OPENSSH",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        var ppk = new StringBuilder();
        ppk.Append("-----BEGIN ").Append(typeName).AppendLine(" PRIVATE KEY-----");
        var bytes = OpenSshPrivateKeyUtilities.EncodePrivateKey(keyPair.Private);
        ppk.AppendLine(Convert.ToBase64String(bytes, Base64FormattingOptions.InsertLineBreaks));
        ppk.Append("-----END ").Append(typeName).AppendLine(" PRIVATE KEY-----");
        return ppk.ToString();
    }

    private static string GeneratePublicKey(string name, AsymmetricCipherKeyPair keyPair, PrivateKeyType type)
    {
        var keyInfo = PrivateKeyInfoFactory.CreatePrivateKeyInfo(keyPair.Private);
        return Convert.ToBase64String(keyInfo.GetDerEncoded());
    }

    public PrivateKeyFile ToPrivateKey()
    {
        PropertyRequired(PrivateKey);
        using var memory = new MemoryStream(Encoding.UTF8.GetBytes(PrivateKey));
        return new PrivateKeyFile(memory);
    }

    public string ToPublicKey()
    {
        PropertyRequired(PublicKey);
        PropertyRequired(Generated);
        var algorithm = Type switch
        {
            PrivateKeyType.Rsa => "ssh-rsa",
            PrivateKeyType.Dsa => "ssh-dss",
            PrivateKeyType.Ecdsa => "ssh-ecdsa",
            PrivateKeyType.Ed22519 => "ssh-ed25519",
            _ => throw new ArgumentOutOfRangeException(nameof(Type), Type, null)
        };
        return $"{algorithm} {PublicKey} {Name}-{Generated:yyyy-MM-dd}";
    }
};