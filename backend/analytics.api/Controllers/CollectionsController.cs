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

        var awaited = await Task.WhenAll(propertyTasks);

        return awaited.SelectMany(x => x).Take(take);

        /*

        let response = await fetch("https://localhost:7087/makeup-shampoo-urls/Silver?take=1000000")
        let data = await response.json()
        data.flatMap(x => x.values).map(x => x.Url)

        */
    }
}
