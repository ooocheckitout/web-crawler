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

using System.Text.Json;

const string collectionsRoot = @"D:\code\web-crawler\collections";

var fileReader = new FileReader();
var fileWriter = new FileWriter();
var downloader = new WebDownloader(new HttpClient(), fileWriter);
var hasher = new Hasher();
var locator = new CollectionLocator(collectionsRoot, hasher);
var parser = new Parser();

var collections = locator.GetCollections().ToList();

foreach (string collection in collections)
{
    var schemas = await fileReader.ReadJsonFileAsync<IEnumerable<Schema>>(
        locator.GetSchemasLocation(collection));

    var urls = (await fileReader.ReadJsonFileAsync<IEnumerable<string>>(
        locator.GetUrlsLocation(collection))).ToList();

    foreach (var schema in schemas)
    {
        // continue only if content or schema has changed
        string schemaHash = hasher.GetSha256HashAsHex(JsonSerializer.Serialize(schema));
        string schemaHashLocation = locator.GetSchemaHashLocation(collection, schema.Name);

        if (File.Exists(schemaHashLocation)
            && await fileReader.ReadTextFileAsync(schemaHashLocation) == schemaHash)
        {
            continue;
        }

        foreach (string url in urls)
        {
            try
            {
                string htmlLocation = locator.GetHtmlLocation(collection, url);

                if (!File.Exists(htmlLocation))
                    await downloader.DownloadTextToFileAsync(url, htmlLocation);

                string htmlContent = await fileReader.ReadTextFileAsync(htmlLocation);

                var objects = parser.Parse(htmlContent, schema);

                string dataLocation = locator.GetDataLocation(collection, schema.Name, url);
                await fileWriter.ToJsonFileAsync(dataLocation, objects);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse {url}");
                Console.WriteLine(ex);
                throw;
            }
        }

        await fileWriter.ToTextFileAsync(schemaHashLocation, schemaHash);
    }
}
