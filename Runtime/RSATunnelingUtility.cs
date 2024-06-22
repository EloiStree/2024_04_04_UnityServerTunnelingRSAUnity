using System.Security.Cryptography;

public class RSATunnelingUtility {

    public static byte[] SignData(byte[] data, RSAParameters privateKey)
    {

        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(privateKey);
            return rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }
    public static bool VerifySignature(byte[] data, byte[] signature, RSAParameters publicKey)
    {
        using (RSA rsa = RSA.Create())
        {
            rsa.ImportParameters(publicKey);
            return rsa.VerifyData(data, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
    }

    public static string GenerateRandomPrivateKeyXml()
    {
        using (RSA rsa = RSA.Create())
        {
            return rsa.ToXmlString(true);
        }
    }

    public static string GetPublicKeyFromPrivateKey(string privateKeyXml)
    {
        using (RSA rsa = RSA.Create()) {
            rsa.FromXmlString(privateKeyXml);
            return rsa.ToXmlString(false);
        }
    }
}