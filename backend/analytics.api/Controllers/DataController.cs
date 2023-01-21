using common;
using common.Collections;
using common.Silver;
using Microsoft.AspNetCore.Mvc;

namespace analytics.api.Controllers;

[ApiController]
[Route("collections/{collection}/data")]
public class DataController : ControllerBase
{
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;

    public DataController(CollectionLocator locator, FileReader fileReader)
    {
        _locator = locator;
        _fileReader = fileReader;
    }

    [HttpGet]
    [Route("{medallion}")]
    public async Task<IEnumerable<Property>> GetDataAsync(string collection, Medallion medallion, CancellationToken cancellationToken, int limit = 100)
    {
        string dataLocation = _locator.GetDataLocation(collection, medallion);

        var properties = new List<Property>();
        foreach (string fileLocation in Directory.EnumerateFiles(dataLocation))
        {
            if (properties.Count >= limit) break;
            if (!System.IO.File.Exists(fileLocation)) continue;

            properties.AddRange(await _fileReader.ReadJsonAsync<IEnumerable<Property>>(fileLocation, cancellationToken));
        }

        var grouped = properties
            .GroupBy(x => x.Name)
            .Select(x => new Property { Name = x.Key, Values = x.SelectMany(y => y.Values).ToList() })
            .Take(limit);

        return grouped;

        /*

        function onlyUnique(value, index, self) {
            return self.indexOf(value) === index;
        }

        let response = await fetch("https://localhost:7087/collections/makeup-shampoo-urls/data/silver?limit=1000000")
        let data = await response.json()
        let detailUrls = data.flatMap(x => x.values).map(x => x.Url).sort()
        let unique = detailUrls.filter(onlyUnique)
        let duplicates = detailUrls.filter((item, index) => detailUrls.indexOf(item) !== index)

        console.log(detailUrls.length, unique.length, duplicates.length)

        */
    }
}
