public class CollectionLocator
{
    private readonly string _collectionsRoot;
    private readonly Hasher _hasher;

    public CollectionLocator(string collectionsRoot, Hasher hasher)
    {
        _collectionsRoot = collectionsRoot;
        _hasher = hasher;
    }

    public IEnumerable<string> GetCollections()
    {
        return Directory.EnumerateDirectories(_collectionsRoot).Select(Path.GetFileName)!;
    }

    public string GetSchemasLocation(string collection)
    {
        return $"{_collectionsRoot}/{collection}/schemas.json";
    }

    public string GetUrlsLocation(string collection)
    {
        return $"{_collectionsRoot}/{collection}/urls.json";
    }

    public string GetHtmlLocation(string collection, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/content/{hash}.html";
    }

    public string GetDataLocation(string collection, string schema, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/data/{schema}/{hash}.json";
    }

    public string GetSchemaHashLocation(string collection, string schema, string htmlFileName)
    {
        return $"{_collectionsRoot}/{collection}/data/{schema}/{htmlFileName}.hash";
    }
}
