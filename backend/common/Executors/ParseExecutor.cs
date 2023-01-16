using common.Bronze;

namespace common.Executors;

public class ParseExecutor
{
    readonly ChecksumCalculator _checksumCalculator;
    readonly Parser _parser;
    readonly FileReader _fileReader;
    readonly FileWriter _fileWriter;

    public ParseExecutor(ChecksumCalculator checksumCalculator, Parser parser, FileReader fileReader, FileWriter fileWriter)
    {
        _checksumCalculator = checksumCalculator;
        _parser = parser;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
    }

    public async Task ParseAsync(string htmlLocation, string dataLocation, string checksumLocation, ParserSchema schema, CancellationToken cancellationToken)
    {
        string htmlContent = await _fileReader.ReadTextAsync(htmlLocation, cancellationToken);

        string checksum = _checksumCalculator.GetParserChecksum(schema, htmlContent);
        if (checksum == await ReadChecksum(checksumLocation, cancellationToken))
            return;

        var bronze = _parser.Parse(htmlContent, schema);
        await _fileWriter.AsJsonAsync(dataLocation, bronze, cancellationToken);
        await _fileWriter.AsTextAsync(checksumLocation, checksum, cancellationToken);
    }

    async Task<string> ReadChecksum(string checksumLocation, CancellationToken cancellationToken)
    {
        return File.Exists(checksumLocation)
            ? await _fileReader.ReadTextAsync(checksumLocation, cancellationToken)
            : string.Empty;
    }
}
