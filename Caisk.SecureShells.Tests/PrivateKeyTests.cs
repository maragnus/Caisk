namespace Caisk.SecureShells.Tests;

[TestClass]
public class PrivateKeyTests
{
    [TestMethod]
    public void GenerateRsa_Works()
    {
        var ppk = PrivateKeyProfile.Generate("Test", PrivateKeyType.Rsa, 2048);
        var privateKey = ppk.ToPrivateKey();
        var publicKey = ppk.ToPublicKey()!;
        
        Assert.AreEqual("ssh-rsa", privateKey.HostKey.Name);
        Assert.AreEqual("ssh-rsa", publicKey[..publicKey.IndexOf(' ')]);
    }
    
    [TestMethod]
    public void GenerateDsa_Works()
    {
        var ppk = PrivateKeyProfile.Generate("Test", PrivateKeyType.Dsa, 1024);
        var privateKey = ppk.ToPrivateKey();
        var publicKey = ppk.ToPublicKey();
        
        Assert.AreEqual("ssh-dss", privateKey.HostKey.Name);
        Assert.AreEqual("ssh-dss", publicKey[..publicKey.IndexOf(' ')]);
    }
    
    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GenerateEcdsa_Works()
    {
        var ppk = PrivateKeyProfile.Generate("Test", PrivateKeyType.Ecdsa, 512);
        var privateKey = ppk.ToPrivateKey();
        var publicKey = ppk.ToPublicKey();
        
        Assert.AreEqual("ssh-ecdsa", privateKey.HostKey.Name);
        Assert.AreEqual("ssh-ecdsa", publicKey[..publicKey.IndexOf(' ')]);
    }
    
    [TestMethod]
    public void GenerateEd22519_Works()
    {
        var ppk = PrivateKeyProfile.Generate("Test", PrivateKeyType.Ed22519);
        var privateKey = ppk.ToPrivateKey();
        var publicKey = ppk.ToPublicKey();
        
        Assert.AreEqual("ssh-ed25519", privateKey.HostKey.Name);
        Assert.AreEqual("ssh-ed25519", publicKey[..publicKey.IndexOf(' ')]);
    }
}