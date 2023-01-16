using common;
using common.Collections;
using Microsoft.AspNetCore.Mvc;

namespace analytics.api.Controllers;

[ApiController]
[Route("collections/{collection}/urls")]
public class UrlsController : ControllerBase
{
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;
    readonly FileWriter _fileWriter;

    public UrlsController(CollectionLocator locator, FileReader fileReader, FileWriter fileWriter)
    {
        _locator = locator;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
    }

    [HttpGet]
    [Route("")]
    public Task<IEnumerable<string>> GetUrls(string collection, CancellationToken cancellationToken)
    {
        string urlsLocation = _locator.GetUrlsFileLocation(collection);
        return _fileReader.ReadJsonAsync<IEnumerable<string>>(urlsLocation, cancellationToken);
    }

    [HttpPost]
    [Route("")]
    public Task CreateUrls(string collection, IEnumerable<string> urls, CancellationToken cancellationToken)
    {
        string urlsLocation = _locator.GetUrlsFileLocation(collection);
        return _fileWriter.AsJsonAsync(urlsLocation, urls, cancellationToken);
    }
}
