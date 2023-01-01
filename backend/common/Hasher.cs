using System.Security.Cryptography;
using System.Text;

public class Hasher
{
    public string GetSha256HashAsHex(string original)
    {
        using var sha256 = SHA256.Create();
        var secretBytes = Encoding.Unicode.GetBytes(original);
        var secretHash = sha256.ComputeHash(secretBytes);
        var hash = Convert.ToHexString(secretHash);

        return hash;
    }
}
