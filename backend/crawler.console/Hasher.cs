using System.Security.Cryptography;
using System.Text;

class Hasher
{
    public string GetSha256HashAsHex(string original)
    {
        using var sha256 = SHA256.Create();
        var secretBytes = Encoding.UTF8.GetBytes(original);
        var secretHash = sha256.ComputeHash(secretBytes);
        var hash = Convert.ToHexString(secretHash);

        Console.WriteLine($"Calculated {hash} hash from {original} string");
        return hash;
    }
}