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

using HtmlAgilityPack;

const string urlFileLocation = "heroes/urls.json";
const string schemaFileLocation = "heroes/schema.json";
const string root = "../../..";

var fileReader = new FileReader();
var fileWriter = new FileWriter();
var downloader = new WebDownloader(new HttpClient(), fileWriter);
var hasher = new Hasher();

var urls = await fileReader.FromJsonFileAsync<IEnumerable<string>>(urlFileLocation);
var fields = await fileReader.FromJsonFileAsync<ICollection<Schema>>(schemaFileLocation);

foreach (var url in urls)
{
    var hash = hasher.GetSha256HashAsHex(url);
    Console.WriteLine($"Writing {url} as {hash}.html file");
    var htmlFileLocation = $"{root}/html/{hash}.html";

    if (!File.Exists(htmlFileLocation))
        await downloader.DownloadTextToFileAsync(url, htmlFileLocation);


    var content = await fileReader.FromTextFileAsync(htmlFileLocation);

    var doc = new HtmlDocument();
    doc.LoadHtml(content);

    var dict = new Dictionary<string, string>();
    foreach (var field in fields)
    {
        var results = doc.DocumentNode.SelectNodes(field.XPath);

        if (results is null || !results.Any())
        {
            Console.WriteLine($"No results for field {field.Name}");
            continue;
        }

        if (results.Count == 1)
        {
            var node = results.Single();
            var value = field.Attribute is not null
                ? node.Attributes[field.Attribute].Value
                : node.InnerText;

            dict.Add(field.Name, value);
        }
    }

    dict.Dump();

    var jsonFileLocation = $"{root}/json/{hash}.json";
    await fileWriter.ToJsonFileAsync(jsonFileLocation, dict);
}