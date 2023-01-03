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
            Schema = await GetSchemasAsync(collectionName)
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

    Task<Schema> GetSchemasAsync(string collectionName)
    {
        string location = _locator.GetSchemasLocation(collectionName);
        return _fileReader.ReadJsonAsync<Schema>(location);
    }
}
