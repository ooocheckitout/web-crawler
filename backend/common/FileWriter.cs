using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;

public class FileWriter
{
    public Task AsJsonAsync(string fileLocation, object obj, CancellationToken cancellationToken)
    {
        string content = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
        return AsTextAsync(fileLocation, content, cancellationToken);
    }

    public async Task AsTextAsync(string fileLocation, string content, CancellationToken cancellationToken)
    {
        // Console.WriteLine($"Writing content to file {fileLocation}");

        Directory.CreateDirectory(Path.GetDirectoryName(fileLocation)!);
        await File.WriteAllTextAsync(fileLocation, content, Encoding.UTF8, cancellationToken);
    }
}
