/*
# list heroes
# list hero details

prepare:
    collect urls
    create schema
execute:
    download html from urls
    retrieve data based on schema
    store data as json files

 */

const string urlFileLocation = "../../../players/urls.json";
const string schemaFileLocation = "../../../players/schema.json";
const string htmlRoot = "../../../html/players";
const string jsonRoot = "../../../json/players";

var fileReader = new FileReader();
var fileWriter = new FileWriter();
var downloader = new WebDownloader(new HttpClient(), fileWriter);
var hasher = new Hasher();
var parser = new Parser();

var urls = await fileReader.ReadJsonFileAsync<IEnumerable<string>>(urlFileLocation);
var schema = await fileReader.ReadJsonFileAsync<Schema>(schemaFileLocation);

foreach (var url in urls)
{
    var hash = hasher.GetSha256HashAsHex(url);
    var htmlFileLocation = $"{htmlRoot}/{hash}.html";

    if (!File.Exists(htmlFileLocation))
        await downloader.DownloadTextToFileAsync(url, htmlFileLocation);
    
    var jsonFileLocation = $"{jsonRoot}/{hash}.json";

#if !DEBUG
    if (File.Exists(jsonFileLocation))
        continue;
#endif

    var content = await fileReader.ReadTextFileAsync(htmlFileLocation);
    var dataObject = parser.Parse(content, schema);
    await fileWriter.ToJsonFileAsync(jsonFileLocation, dataObject);
}