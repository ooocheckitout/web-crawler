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

foreach (string collection in locator.GetCollections())
{
    var urls = await fileReader.ReadJsonFileAsync<IReadOnlyCollection<string>>(locator.GetUrlsLocation(collection));
    var schemas = await fileReader.ReadJsonFileAsync<IReadOnlyCollection<Schema>>(locator.GetSchemasLocation(collection));

    foreach (string url in urls)
    {
        string htmlLocation = locator.GetHtmlLocation(collection, url);

        if (!File.Exists(htmlLocation))
            await downloader.DownloadTextToFileAsync(url, htmlLocation);

        string htmlContent = await fileReader.ReadTextFileAsync(htmlLocation);

        foreach (var schema in schemas)
        {
            try
            {
                // continue only if content or schema has changed
                string schemaHash = hasher.GetSha256HashAsHex(JsonSerializer.Serialize(schema));
                string schemaHashLocation = locator.GetSchemaHashLocation(collection, schema.Name, Path.GetFileNameWithoutExtension(htmlLocation));

                if (File.Exists(schemaHashLocation)
                    && await fileReader.ReadTextFileAsync(schemaHashLocation) == schemaHash)
                {
                    Console.WriteLine($"Skipping schema {schema.Name}");
                    continue;
                }

                var objects = parser.Parse(htmlContent, schema);

                string dataLocation = locator.GetDataLocation(collection, schema.Name, url);
                await fileWriter.ToJsonFileAsync(dataLocation, objects);

                await fileWriter.ToTextFileAsync(schemaHashLocation, schemaHash);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to parse {url} with {schema.Name}");
                Console.WriteLine(ex);
            }
        }
    }
}
