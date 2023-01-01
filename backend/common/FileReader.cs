using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

public class FileReader
{
    public async Task<T> ReadJsonFileAsync<T>(string fileLocation)
    {
        var content = await ReadTextFileAsync(fileLocation);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
               {
                   PropertyNameCaseInsensitive = true,
                   Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
               })
               ?? throw new InvalidOperationException("File content is not compatible with the schema");
    }

    public async Task<string> ReadTextFileAsync(string fileLocation)
    {
        // Console.WriteLine($"Reading content from file {fileLocation}");
        return await File.ReadAllTextAsync(fileLocation, Encoding.UTF8);
    }
}
