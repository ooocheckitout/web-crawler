using System.Diagnostics;
using common.Silver;
using Microsoft.Extensions.Logging;

namespace common.Executors;

public class TransformExecutor
{
    readonly ChecksumCalculator _checksumCalculator;
    readonly Transformer _transformer;
    readonly FileReader _fileReader;
    readonly FileWriter _fileWriter;
    readonly ILogger<TransformExecutor> _logger;

    public TransformExecutor(
        ChecksumCalculator checksumCalculator, Transformer transformer, FileReader fileReader, FileWriter fileWriter, ILogger<TransformExecutor> logger)
    {
        _checksumCalculator = checksumCalculator;
        _transformer = transformer;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
        _logger = logger;
    }


    public async Task TransformAsync(string bronzeLocation, string dataLocation, string checksumLocation, TransformerSchema schema,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Start transforming from {bronzeLocation} to {dataLocation}", bronzeLocation, dataLocation);

        var bronze = (await _fileReader.ReadJsonAsync<IEnumerable<Property>>(bronzeLocation, cancellationToken)).ToList();
        string checksum = _checksumCalculator.GetTransformerChecksum(schema, bronze);

        if (checksum == await ReadChecksum(checksumLocation, cancellationToken))
        {
            _logger.LogInformation("Checksums match. Skip transforming from {bronzeLocation} to {dataLocation}", bronzeLocation, dataLocation);
            return;
        }

        var silver = _transformer.Transform(bronze, schema);
        await _fileWriter.AsJsonAsync(dataLocation, silver, cancellationToken);
        await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);

        _logger.LogInformation("Finish transforming from {bronzeLocation} to {dataLocation}", bronzeLocation, dataLocation);
    }

    async Task<string> ReadChecksum(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }
}
