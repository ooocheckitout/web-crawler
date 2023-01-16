using common;
using common.Bronze;
using common.Collections;
using common.Silver;
using Microsoft.AspNetCore.Mvc;

namespace analytics.api.Controllers;

[ApiController]
[Route("collections/{collection}/schemas")]
public class SchemasController : ControllerBase
{
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;
    readonly FileWriter _fileWriter;

    public SchemasController(CollectionLocator locator, FileReader fileReader, FileWriter fileWriter)
    {
        _locator = locator;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
    }

    [HttpGet]
    [Route("bronze")]
    public Task<ParserSchema> GetParserSchema(string collection, CancellationToken cancellationToken)
    {
        string schemaLocation = _locator.GetSchemaLocation(collection, Medallion.Bronze);
        return _fileReader.ReadJsonAsync<ParserSchema>(schemaLocation, cancellationToken);
    }

    [HttpPost]
    [Route("bronze")]
    public Task CreateParserSchema(string collection, ParserSchema schema, CancellationToken cancellationToken)
    {
        string schemaLocation = _locator.GetSchemaLocation(collection, Medallion.Bronze);
        return _fileWriter.AsJsonAsync(schemaLocation, schema, cancellationToken);
    }

    [HttpGet]
    [Route("silver")]
    public Task<TransformerSchema> GetTransformSchema(string collection, CancellationToken cancellationToken)
    {
        string schemaLocation = _locator.GetSchemaLocation(collection, Medallion.Silver);
        return _fileReader.ReadJsonAsync<TransformerSchema>(schemaLocation, cancellationToken);
    }

    [HttpPost]
    [Route("silver")]
    public Task CreateTransformSchema(string collection, ParserSchema schema, CancellationToken cancellationToken)
    {
        string schemaLocation = _locator.GetSchemaLocation(collection, Medallion.Silver);
        return _fileWriter.AsJsonAsync(schemaLocation, schema, cancellationToken);
    }
}
