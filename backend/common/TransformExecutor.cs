using common.Silver;

namespace common;

public class TransformExecutor
{
    readonly ChecksumCalculator _checksumCalculator;
    readonly CollectionLocator _locator;
    readonly Transformer _transformer;
    readonly FileReader _fileReader;
    readonly FileWriter _fileWriter;

    public TransformExecutor(
        ChecksumCalculator checksumCalculator, CollectionLocator locator, Transformer transformer, FileReader fileReader, FileWriter fileWriter)
    {
        _checksumCalculator = checksumCalculator;
        _locator = locator;
        _transformer = transformer;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
    }


    public async Task TransformAsync(string bronzeLocation, string dataLocation, string checksumLocation, TransformerSchema schema,
        CancellationToken cancellationToken)
    {
        var bronze = (await _fileReader.ReadJsonAsync<IEnumerable<Property>>(bronzeLocation, cancellationToken)).ToList();
        string checksum = _checksumCalculator.GetTransformerChecksum(schema, bronze);

        if (checksum == await ReadChecksum(checksumLocation, cancellationToken))
            return;

        var silver = _transformer.Transform(bronze, schema);
        await _fileWriter.AsJsonAsync(dataLocation, silver, cancellationToken);
        await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);
    }

    async Task<string> ReadChecksum(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }
}
