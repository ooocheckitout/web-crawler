using System.Text.Json;

class FileReader
{
    public async Task<T> FromJsonFileAsync<T>(string location)
    {
        var content = await File.ReadAllTextAsync(location);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
               ?? throw new InvalidOperationException("File content is not compatible with the schema");
    }

    public async Task<string> FromTextFileAsync(string location)
    {
        return await File.ReadAllTextAsync(location);
    }
}