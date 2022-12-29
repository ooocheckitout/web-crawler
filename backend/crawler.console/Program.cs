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

var fileReader = new FileReader();
var fileWriter = new FileWriter();
var downloader = new WebDownloader(new HttpClient(), fileWriter);
var hasher = new Hasher();
var collectionManager = new CollectionManager(fileReader, downloader, hasher, fileWriter);
var parser = new Parser();

var collections = new[] { /* "heroes", "details", */ "statistics" };
foreach (var collection in collections)
{
    foreach (var url in await collectionManager.GetUrlsAsync(collection))
    {
        var htmlContent = await collectionManager.GetOrCreateHtmlContentAsync(collection, url);

        var schema = await collectionManager.GetSchemaAsync(collection);
        var dataObjects = parser.Parse(htmlContent, schema);

        await collectionManager.CreateDataAsync(collection, url, dataObjects);
    }
}