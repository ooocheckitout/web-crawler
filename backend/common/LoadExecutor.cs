namespace common;

public class LoadExecutor
{
    readonly FileWriter _fileWriter;
    readonly Pool<SeleniumDownloader> _downloaderPool;

    public LoadExecutor(FileWriter fileWriter, Pool<SeleniumDownloader> downloaderPool)
    {
        _fileWriter = fileWriter;
        _downloaderPool = downloaderPool;
    }

    public async Task LoadContentAsync(string url, string htmlLocation, CancellationToken cancellationToken)
    {
        if (File.Exists(htmlLocation))
            return;

        using var downloader = _downloaderPool.GetAvailableOrWait();
        string htmlContent = await ((SeleniumDownloader) downloader).DownloadAsTextAsync(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);
    }
}
