public class CollectionFactory
{
    private readonly CollectionLocator _locator;
    private readonly FileReader _fileReader;

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
            Schemas = await GetSchemasAsync(collectionName)
        };
    }

    public Task<Collection[]> GetAllAsync()
    {
        var collections = Directory.EnumerateDirectories(_locator.GetRoot()).Select(x => Path.GetFileName(x)!);
        return Task.WhenAll(collections.Select(GetSingleAsync));
    }

    private Task<IEnumerable<string>> GetUrlsAsync(string collectionName)
    {
        string location = _locator.GetUrlsLocation(collectionName);
        return _fileReader.ReadJsonAsync<IEnumerable<string>>(location);
    }

    private Task<IEnumerable<Schema>> GetSchemasAsync(string collectionName)
    {
        string location = _locator.GetSchemasLocation(collectionName);
        return _fileReader.ReadJsonAsync<IEnumerable<Schema>>(location);
    }
}
