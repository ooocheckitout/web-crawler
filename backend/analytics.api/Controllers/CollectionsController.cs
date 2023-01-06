using System.Runtime.CompilerServices;
using common;
using common.Silver;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Spark.Sql;
using System.Linq;

namespace analytics.api.Controllers;

[ApiController]
[Route("[controller]")]
public class CollectionsController : ControllerBase
{
    readonly SparkSession _sparkSession;
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;

    public CollectionsController(SparkSession sparkSession, CollectionLocator locator, FileReader fileReader)
    {
        _sparkSession = sparkSession;
        _locator = locator;
        _fileReader = fileReader;
    }

    [HttpGet]
    [Route("/{collection}/{medallion}")]
    public async Task<List<Property>> GetAsync(
        string collection, Medallion medallion, CancellationToken cancellationToken, int take = 100)
    {
        var properties = new List<Property>();

        string dataLocation = _locator.GetDataLocation(collection, medallion);
        foreach (string fileLocation in Directory.EnumerateFiles(dataLocation).Take(take))
        {
            var fileProperties = await _fileReader.ReadJsonAsync<IEnumerable<Property>>(fileLocation, cancellationToken);

            foreach (var fileProperty in fileProperties)
            {
                var property = properties.Find(x => x.Name == fileProperty.Name);
                if (property == null)
                {
                    properties.Add(fileProperty);
                }
                else
                {
                    property.Values.AddRange(fileProperty.Values);
                }
            }
        }

        return properties;
    }
}
