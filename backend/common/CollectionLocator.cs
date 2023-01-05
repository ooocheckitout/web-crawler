public class CollectionLocator
{
    readonly string _collectionsRoot;
    readonly Hasher _hasher;

    public CollectionLocator(string collectionsRoot, Hasher hasher)
    {
        _collectionsRoot = collectionsRoot;
        _hasher = hasher;
    }

    public string GetRoot()
    {
        return _collectionsRoot;
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

    public string GetBronzeLocation(string collection)
    {
        return $"{_collectionsRoot}/{collection}/bronze";
    }

    public string GetBronzeFileLocation(string collection, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/bronze/{hash}.json";
    }

    public string GetChecksumLocation(string collection, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/checksum/{hash}.checksum";
    }
}
