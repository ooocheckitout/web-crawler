using System.Security.Cryptography;
using System.Text;

public class Hasher
{
    public string GetSha256HashAsHex(string original)
    {
        using var sha256 = SHA256.Create();
        byte[] secretBytes = Encoding.Unicode.GetBytes(original);
        byte[] secretHash = sha256.ComputeHash(secretBytes);
        string hash = Convert.ToHexString(secretHash);

        return hash;
    }
}
