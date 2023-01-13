using common.Bronze;
using common.Silver;

namespace common;

public class ParseExecutor
{
    readonly ChecksumCalculator _checksumCalculator;
    readonly CollectionLocator _locator;
    readonly Parser _parser;
    readonly FileReader _fileReader;
    readonly FileWriter _fileWriter;

    public ParseExecutor(ChecksumCalculator checksumCalculator, CollectionLocator locator, Parser parser, FileReader fileReader, FileWriter fileWriter)
    {
        _checksumCalculator = checksumCalculator;
        _locator = locator;
        _parser = parser;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
    }

    public async Task<IEnumerable<Property>> ParseAsync(string collectionName, string url, ParserSchema schema, string htmlContent,
        CancellationToken cancellationToken)
    {
        string checksum = _checksumCalculator.GetParserChecksum(schema, htmlContent);
        string checksumLocation = _locator.GetChecksumLocation(collectionName, url, Medallion.Bronze);

        string dataFileLocation = _locator.GetDataFileLocation(collectionName, url, Medallion.Bronze);
        if (checksum == await ReadChecksum(checksumLocation, cancellationToken))
            return await _fileReader.ReadJsonAsync<IEnumerable<Property>>(dataFileLocation, cancellationToken);

        var bronze = _parser.Parse(htmlContent, schema);
        await _fileWriter.AsJsonAsync(dataFileLocation, bronze, cancellationToken);
        await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);

        return bronze;
    }

    async Task<string> ReadChecksum(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }
}
