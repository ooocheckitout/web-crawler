using common;
using common.Bronze;
using common.Silver;

public class CollectionFactory
{
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;

    public CollectionFactory(CollectionLocator locator, FileReader fileReader)
    {
        _locator = locator;
        _fileReader = fileReader;
    }

    public async Task<Collection> GetSingleAsync(string collectionName)
    {
        return new Collection
        {
            Name = collectionName,
            Urls = await GetUrlsAsync(collectionName),
            ParserSchema = await GetParserSchemasAsync(collectionName),
            TransformerSchema = await GetTransformerSchemasAsync(collectionName)
        };
    }

    public Task<Collection[]> GetAllAsync()
    {
        var collections = Directory.EnumerateDirectories(_locator.GetRoot()).Select(x => Path.GetFileName(x)!);
        return Task.WhenAll(collections.Select(GetSingleAsync));
    }

    Task<IEnumerable<string>> GetUrlsAsync(string collectionName)
    {
        string location = _locator.GetUrlsLocation(collectionName);
        return _fileReader.ReadJsonAsync<IEnumerable<string>>(location);
    }

    Task<ParserSchema> GetParserSchemasAsync(string collectionName)
    {
        string location = _locator.GetSchemaLocation(collectionName, Medallion.Bronze);
        return _fileReader.ReadJsonAsync<ParserSchema>(location);
    }

    Task<TransformerSchema> GetTransformerSchemasAsync(string collectionName)
    {
        string location = _locator.GetSchemaLocation(collectionName, Medallion.Silver);
        return _fileReader.ReadJsonAsync<TransformerSchema>(location);
    }
}
