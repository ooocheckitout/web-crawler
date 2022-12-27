using System.Security.Cryptography;
using System.Text;

class Hasher
{
    public string GetSha256HashAsHex(string secret)
    {
        using var sha256 = SHA256.Create();
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var secretHash = sha256.ComputeHash(secretBytes);
        return Convert.ToHexString(secretHash);
    }
}