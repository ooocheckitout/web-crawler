using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;

public class FileWriter
{
    public Task AsJsonAsync(string fileLocation, object obj)
    {
        string content = JsonSerializer.Serialize(obj, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
        });
        return AsTextAsync(fileLocation, content);
    }

    public async Task AsTextAsync(string fileLocation, string content)
    {
        // Console.WriteLine($"Writing content to file {fileLocation}");

        Directory.CreateDirectory(Path.GetDirectoryName(fileLocation)!);
        await File.WriteAllTextAsync(fileLocation, content, Encoding.UTF8);
    }
}
