using System.Security.Cryptography;
using System.Text;
using OtpNet;
using QRCoder;

namespace Caisk.Users;

[PublicAPI]
public static class UserUtility
{
    public const string IssuerName = "Caisk";
    public const int TotpSecretLength = 20;
    public const int TotpInterval = 30;
    public const int TotpCodeLength = 6;
    public const OtpHashMode TotpMode = OtpHashMode.Sha512;
    public const int WindowPrevious = 1; // RFC 6238 Section 5.2 recommends 1
    public const int WindowFuture = 1; // RFC 6238 Section 5.2 recommends 1
    private const string AllowedChars = "ABCDEFGHJKMNPQRSTUVWXYZabcdefghkmnpqrstuvwxyz23456789!@#$%^&*-_";

    public static string HashPassword(string password, string salt) =>
        Convert.ToHexString(SHA256.HashData(SHA512.HashData(Encoding.UTF8.GetBytes(salt + password))));

    public static bool VerifyTotp(DateTime utcTimeStamp, string secret, string code)
    {
        var secretKey = Base32Encoding.ToBytes(secret);
        var totp = new OtpNet.Totp(secretKey, mode: TotpMode, step: TotpInterval);
        return totp.VerifyTotp(utcTimeStamp.ToUniversalTime(), code, out _,
            new VerificationWindow(WindowPrevious, WindowFuture));
    }

    public static string GeneratePassword(int minLength = 9, int maxLength = 13)
    {
        var length = RandomNumberGenerator.GetInt32(minLength, maxLength);
        return RandomNumberGenerator.GetString(AllowedChars, length);
    }

    public static string GenerateTotpSecret() =>
        Base32Encoding.ToString(KeyGeneration.GenerateRandomKey(20));

    public static byte[] TotpQrCodePng(string label, string secret)
    {
        var payload = TotpToUrl(label, secret).ToString();
        using var gen = new QRCodeGenerator();
        using var data = gen.CreateQrCode(payload, QRCodeGenerator.ECCLevel.L);
        using var png = new PngByteQRCode(data);
        return png.GetGraphic(20);
    }

    public static Uri TotpToUrl(string label, string secret) =>
        new OtpUri(OtpType.Totp, secret, label, IssuerName, TotpMode, TotpCodeLength, TotpInterval).ToUri();

    public static string GenerateCode(DateTime utcTimeStamp, string secret)
    {
        var totp = new Totp(Base32Encoding.ToBytes(secret), UserUtility.TotpInterval, UserUtility.TotpMode,
            UserUtility.TotpCodeLength);
        return totp.ComputeTotp(utcTimeStamp.ToUniversalTime());
    }
}