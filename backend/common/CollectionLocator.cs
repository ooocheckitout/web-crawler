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
        return _collectionsRoot;
    }

    public string GetSchemaLocation(string collection, Medallion medallion)
    {
        return $"{_collectionsRoot}/{collection}/{MedallionToString(medallion)}.json";
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

    public string GetDataLocation(string collection, Medallion medallion)
    {
        return $"{_collectionsRoot}/{collection}/{MedallionToString(medallion)}";
    }

    public string GetDataFileLocation(string collection, string url, Medallion medallion)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/{MedallionToString(medallion)}/{hash}.json";
    }

    public string GetChecksumLocation(string collection, string url, Medallion medallion)
    {
        string hash = _hasher.GetSha256HashAsHex(url);
        return $"{_collectionsRoot}/{collection}/checksum/{MedallionToString(medallion)}/{hash}.checksum";
    }

    public string GetComponentsFileLocation(string collection)
    {
        return $"{_collectionsRoot}/{collection}/checksum/components.txt";
    }

    string MedallionToString(Medallion medallion)
    {
        return medallion.ToString().ToLower();
    }
}
