using common;
using common.Silver;
using Microsoft.AspNetCore.Mvc;

namespace analytics.api.Controllers;

[ApiController]
[Route("[controller]")]
public class CollectionsController : ControllerBase
{
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;

    public CollectionsController(CollectionLocator locator, FileReader fileReader)
    {
        _locator = locator;
        _fileReader = fileReader;
    }

    [HttpGet]
    [Route("{collection}/{medallion}")]
    public async Task<IEnumerable<Property>> GetAsync(string collection, Medallion medallion, CancellationToken cancellationToken, int take = 100)
    {
        string dataLocation = _locator.GetDataLocation(collection, medallion);

        var propertyTasks = Directory
            .EnumerateFiles(dataLocation)
            .Select(fileLocation => _fileReader.ReadJsonAsync<IEnumerable<Property>>(fileLocation, cancellationToken));

        var properties = await Task.WhenAll(propertyTasks);

        var grouped = properties
            .SelectMany(x => x)
            .GroupBy(x => x.Name)
            .Select(x => new Property { Name = x.Key, Values = x.SelectMany(y => y.Values).ToList() });

        return grouped;

        /*

        function onlyUnique(value, index, self) {
            return self.indexOf(value) === index;
        }

        let response = await fetch("https://localhost:7087/collections/makeup-shampoo-urls/Silver?take=1000000")
        let data = await response.json()
        let detailUrls = data.flatMap(x => x.values).map(x => x.Url).filter(onlyUnique)

        */
    }
}
