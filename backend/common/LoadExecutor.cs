namespace common;

public class LoadExecutor
{
    readonly CollectionLocator _locator;
    readonly FileReader _fileReader;
    readonly FileWriter _fileWriter;
    readonly SeleniumDownloader _downloader;

    public LoadExecutor(CollectionLocator locator, FileReader fileReader, FileWriter fileWriter, SeleniumDownloader downloader)
    {
        _locator = locator;
        _fileReader = fileReader;
        _fileWriter = fileWriter;
        _downloader = downloader;
    }

    public async Task<string> LoadContentsAsync(string collectionName, string url, CancellationToken cancellationToken)
    {
        string htmlLocation = _locator.GetHtmlLocation(collectionName, url);

        if (File.Exists(htmlLocation))
            return await _fileReader.ReadTextAsync(htmlLocation, cancellationToken);

        string htmlContent = await _downloader.DownloadAsTextAsync(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);

        string componentsLocation = _locator.GetComponentsFileLocation(collectionName);
        string components = await _fileReader.ReadTextAsync(componentsLocation, cancellationToken);

        var componentString = $"{url} {htmlLocation}";
        string content = string.Join(Environment.NewLine, components.Split(Environment.NewLine).Concat(new[] { componentString }));
        await _fileWriter.AsTextAsync(componentsLocation, content, cancellationToken);

        return htmlContent;
    }
}
