using System.Web;
using OtpNet;

namespace Caisk.Users.Tests;

[TestClass]
public class UtilityTests
{
    [TestMethod]
    public void VerifyTotp()
    {
        var now = new DateTime(2024, 1, 1, 12, 0, 0);
        var secret = UserUtility.GenerateTotpSecret();
        Assert.AreEqual(32, secret.Length, $"Secret length expected to be {UserUtility.TotpSecretLength} characters");

        var totp = new Totp(Base32Encoding.ToBytes(secret), UserUtility.TotpInterval, UserUtility.TotpMode,
            UserUtility.TotpCodeLength);
        var code = totp.ComputeTotp(now);
        Assert.AreEqual(UserUtility.TotpCodeLength, code.Length);

        Assert.IsTrue(UserUtility.VerifyTotp(now, secret, code));
    }

    [TestMethod]
    public void VerifyTotpUrl()
    {
        var now = new DateTime(2024, 1, 1, 12, 0, 0);
        var secret = UserUtility.GenerateTotpSecret();
        Assert.AreEqual(32, secret.Length, $"Secret length expected to be {UserUtility.TotpSecretLength} characters");

        var uri = UserUtility.TotpToUrl("UserName", secret);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var urlSecret = query["secret"];
        Assert.AreEqual(secret, urlSecret);

        var totp = new Totp(Base32Encoding.ToBytes(urlSecret), UserUtility.TotpInterval, UserUtility.TotpMode,
            UserUtility.TotpCodeLength);
        var code = totp.ComputeTotp(now);
        Assert.AreEqual(UserUtility.TotpCodeLength, code.Length);

        Assert.IsTrue(UserUtility.VerifyTotp(now, secret, code));
    }
}