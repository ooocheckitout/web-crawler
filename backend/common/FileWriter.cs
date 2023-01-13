using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Unicode;
using Microsoft.Extensions.Logging;

namespace common;

public class FileWriter
{
    readonly ILogger<FileWriter> _logger;

    public FileWriter(ILogger<FileWriter> logger)
    {
        _logger = logger;
    }

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
        _logger.LogDebug("Writing to {fileLocation}", fileLocation);
        Directory.CreateDirectory(Path.GetDirectoryName(fileLocation)!);
        await File.WriteAllTextAsync(fileLocation, content, Encoding.UTF8, cancellationToken);
    }
}
