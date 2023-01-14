namespace common;

public enum Medallion
{
    None = 0,
    Bronze = 1,
    Silver = 2,
}

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
        return Path.GetFullPath(_collectionsRoot);
    }

    public string GetSchemaLocation(string collection, Medallion medallion)
    {
        return Path.GetFullPath($"{collection}/{MedallionToString(medallion)}.json", _collectionsRoot);
    }

    public string GetUrlsLocation(string collection)
    {
        return Path.GetFullPath($"{collection}/urls.json", _collectionsRoot);
    }

    public string GetHtmlLocation(string collection, string url)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return Path.GetFullPath($"{collection}/content/{hash}.html", _collectionsRoot);
    }

    public string GetDataLocation(string collection, Medallion medallion)
    {
        return Path.GetFullPath($"{collection}/{MedallionToString(medallion)}", _collectionsRoot);
    }

    public string GetDataFileLocation(string collection, string url, Medallion medallion)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return Path.GetFullPath($"{collection}/{MedallionToString(medallion)}/{hash}.json", _collectionsRoot);
    }

    public string GetChecksumLocation(string collection, string url, Medallion medallion)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return Path.GetFullPath($"{collection}/checksum/{MedallionToString(medallion)}/{hash}.checksum", _collectionsRoot);
    }

    public string GetComponentsFileLocation(string collection)
    {
        return Path.GetFullPath($"{collection}/checksum/components.txt", _collectionsRoot);
    }

    static string MedallionToString(Medallion medallion)
    {
        return medallion.ToString().ToLower();
    }
}
