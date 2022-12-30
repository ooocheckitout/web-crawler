/*
# list heroes
# list hero details
# list player details

prepare:
    collect urls
    create schema
execute:
    download html from urls
    retrieve data based on schema
    store data as json files

 */

const string collectionsRoot = @"D:\code\web-crawler\collections";

var fileReader = new FileReader();
var fileWriter = new FileWriter();
var downloader = new WebDownloader(new HttpClient(), fileWriter);
var hasher = new Hasher();
var collectionManager = new CollectionManager(collectionsRoot, fileReader, hasher, downloader, fileWriter);
var parser = new Parser();


foreach (string collection in collectionManager.GetCollections())
foreach (string url in await collectionManager.GetUrlsAsync(collection))
{
    try
    {
        string html = await collectionManager.GetHtmlAsync(collection, url);

        foreach (var schema in await collectionManager.GetSchemasAsync(collection))
        {
            var objects = parser.Parse(html, schema);
            await collectionManager.SaveDataAsync(collection, schema.Name, url, objects);
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Failed to parse {url}");
        Console.WriteLine(ex);
        throw;
    }
}
