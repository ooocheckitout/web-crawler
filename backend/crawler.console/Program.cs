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

var collections = new[]
{
    "heroes",
    "details",
    "statistics"
};

foreach (string collection in collections)
foreach (string url in await collectionManager.GetUrlsAsync(collection))
{
    string htmlContent = await collectionManager.GetOrCreateHtmlContentAsync(collection, url);

    var schemas = await collectionManager.GetSchemaAsync(collection);
    foreach (var schema in schemas)
    {
        var dataObjects = parser.Parse(htmlContent, schema);
        await collectionManager.CreateDataAsync(collection, schema.Name, url, dataObjects);
    }


}
