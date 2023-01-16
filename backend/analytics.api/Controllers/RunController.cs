using common;
using common.Collections;
using Microsoft.AspNetCore.Mvc;

namespace analytics.api.Controllers;

[ApiController]
[Route("collections/{collection}/run")]
public class RunController : ControllerBase
{
    readonly CollectionFactory _factory;
    readonly ParallelCollectionRunner _runner;

    public RunController(CollectionFactory factory, ParallelCollectionRunner runner)
    {
        _factory = factory;
        _runner = runner;
    }

    [HttpPut]
    [Route("")]
    public async Task Run(string collection, CancellationToken cancellationToken)
    {
        var collectionObject = await _factory.GetSingleAsync(collection, cancellationToken);
        await _runner.RunAsync(collectionObject, cancellationToken);
    }
}
