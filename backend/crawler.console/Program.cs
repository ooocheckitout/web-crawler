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

const string urlFileLocation = "details/urls.json";
const string schemaFileLocation = "details/schema.json";
const string htmlRoot = "../../../html/details";
const string jsonRoot = "../../../json/details";

var fileReader = new FileReader();
var fileWriter = new FileWriter();
var downloader = new WebDownloader(new HttpClient(), fileWriter);
var hasher = new Hasher();
var parser = new Parser();

var urls = await fileReader.FromJsonFileAsync<IEnumerable<string>>(urlFileLocation);
var schema = await fileReader.FromJsonFileAsync<Schema>(schemaFileLocation);

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

    var content = await fileReader.FromTextFileAsync(htmlFileLocation);
    if (schema.HasMultipleResultsPerPage)
    {
        var multipleObject = parser.ParseMultipleObject(content, schema.Fields);
        await fileWriter.ToJsonFileAsync(jsonFileLocation, multipleObject);
    }
    else
    {
        var singleObject = parser.ParseSingleObject(content, schema.Fields);
        await fileWriter.ToJsonFileAsync(jsonFileLocation, singleObject);
    }
}