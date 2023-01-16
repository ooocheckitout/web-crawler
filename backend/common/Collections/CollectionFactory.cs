using common.Bronze;
using common.Silver;

namespace common.Collections;

public class CollectionFactory
{
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;

    public CollectionFactory(CollectionLocator locator, FileReader fileReader)
    {
        _locator = locator;
        _fileReader = fileReader;
    }

    public async Task<Collection> GetSingleAsync(string collectionName, CancellationToken cancellationToken)
    {
        return new Collection
        {
            Name = collectionName,
            Urls = await GetUrlsAsync(collectionName, cancellationToken),
            ParserSchema = await GetParserSchemasAsync(collectionName, cancellationToken),
            TransformerSchema = await GetTransformerSchemasAsync(collectionName, cancellationToken)
        };
    }

    public Task<Collection[]> GetAllAsync(CancellationToken cancellationToken)
    {
        var collections = Directory.EnumerateDirectories(_locator.GetRoot()).Select(x => Path.GetFileName(x)!);
        return Task.WhenAll(collections.Select(x => GetSingleAsync(x, cancellationToken)));
    }

    Task<IEnumerable<string>> GetUrlsAsync(string collectionName, CancellationToken cancellationToken)
    {
        string location = _locator.GetUrlsFileLocation(collectionName);
        return _fileReader.ReadJsonAsync<IEnumerable<string>>(location, cancellationToken);
    }

    Task<ParserSchema> GetParserSchemasAsync(string collectionName, CancellationToken cancellationToken)
    {
        string location = _locator.GetSchemaFileLocation(collectionName, Medallion.Bronze);
        return _fileReader.ReadJsonAsync<ParserSchema>(location, cancellationToken);
    }

    Task<TransformerSchema> GetTransformerSchemasAsync(string collectionName, CancellationToken cancellationToken)
    {
        string location = _locator.GetSchemaFileLocation(collectionName, Medallion.Silver);
        return _fileReader.ReadJsonAsync<TransformerSchema>(location, cancellationToken);
    }
}
