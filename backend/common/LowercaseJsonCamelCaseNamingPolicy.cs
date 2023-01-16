using System.Text.Json;

namespace common;

public class LowercaseJsonCamelCaseNamingPolicy : JsonNamingPolicy
{
    readonly JsonNamingPolicy _internal = CamelCase;

    public override string ConvertName(string name)
    {
        return LowercaseFirstLetter(_internal.ConvertName(name));
    }

    static string LowercaseFirstLetter(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        return char.ToLower(name[0]) + name[1..];
    }
}
