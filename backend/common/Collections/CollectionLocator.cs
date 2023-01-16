namespace common.Collections;

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

    public string GetLockFileLocation(string collection)
    {
        return Path.GetFullPath($"{collection}/.lock", _collectionsRoot);
    }

    public string GetSchemaFileLocation(string collection, Medallion medallion)
    {
        return Path.GetFullPath($"{collection}/{MedallionToString(medallion)}.json", _collectionsRoot);
    }

    public string GetUrlsFileLocation(string collection)
    {
        return Path.GetFullPath($"{collection}/urls.json", _collectionsRoot);
    }

    public string GetHtmlFileLocation(string collection, string url)
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

    public string GetChecksumFileLocation(string collection, string url, Medallion medallion)
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
