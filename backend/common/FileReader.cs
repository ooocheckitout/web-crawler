using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using Microsoft.Extensions.Logging;

namespace common;

public class FileReader
{
    readonly ILogger<FileReader> _logger;

    public FileReader(ILogger<FileReader> logger)
    {
        _logger = logger;
    }

    public async Task<T> ReadJsonAsync<T>(string fileLocation, CancellationToken cancellationToken)
    {
        string content = await ReadTextAsync(fileLocation, cancellationToken);
        return JsonSerializer.Deserialize<T>(content, new JsonSerializerOptions
               {
                   PropertyNameCaseInsensitive = true,
                   Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
               })
               ?? throw new InvalidOperationException("File content is not compatible with the schema");
    }

    public async Task<string> ReadTextAsync(string fileLocation, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Reading from {fileLocation}", fileLocation);
        return await File.ReadAllTextAsync(fileLocation, Encoding.UTF8, cancellationToken);
    }
}
