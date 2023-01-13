using System.Security.Cryptography;
using System.Text;

namespace common;

public class Hasher
{
    public string GetSha256HashAsHex(string originalString)
    {
        using var sha256 = SHA256.Create();
        byte[] secretBytes = Encoding.Unicode.GetBytes(originalString);
        byte[] secretHash = sha256.ComputeHash(secretBytes);
        return Convert.ToHexString(secretHash);
    }

    public string GetSha256HashAsHex(params string[] strings)
    {
        var hashes = strings.Select(GetSha256HashAsHex);
        return GetSha256HashAsHex(string.Join("", hashes));
    }
}
