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


    public async Task<IEnumerable<Property>> TransformAsync(
        string collectionName, string url, TransformerSchema schema, ICollection<Property> bronze, CancellationToken cancellationToken)
    {
        string checksum = _checksumCalculator.GetTransformerChecksum(schema, bronze);
        string checksumLocation = _locator.GetChecksumLocation(collectionName, url, Medallion.Silver);

        string dataFileLocation = _locator.GetDataFileLocation(collectionName, url, Medallion.Silver);
        if (checksum == await ReadChecksum(checksumLocation, cancellationToken))
            return await _fileReader.ReadJsonAsync<IEnumerable<Property>>(dataFileLocation, cancellationToken);

        var silver = _transformer.Transform(bronze, schema);
        await _fileWriter.AsJsonAsync(dataFileLocation, silver, cancellationToken);
        await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);

        return silver;
    }

    async Task<string> ReadChecksum(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }
}
