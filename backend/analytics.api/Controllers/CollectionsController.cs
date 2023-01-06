using common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Spark.Sql;

namespace analytics.api.Controllers;

[ApiController]
[Route("[controller]")]
public class CollectionsController : ControllerBase
{
    readonly SparkSession _sparkSession;
    readonly CollectionLocator _locator;

    public CollectionsController(SparkSession sparkSession, CollectionLocator locator)
    {
        _sparkSession = sparkSession;
        _locator = locator;
    }

    [HttpGet]
    public IEnumerable<object> Bronze(string collectionName)
    {
        var results = _sparkSession
            .Read()
            .Option("multiline", true)
            .Json(_locator.GetDataLocation(collectionName, Medallion.Bronze))
            .Collect().ToList();

        foreach (var item in results)
        {
            for (var j = 0; j < item.Schema.Fields.Count; j++)
            {
                yield return new { Name = item.Schema.Fields[j].Name, Value = item.Values[j] };
            }
        }
    }
}
