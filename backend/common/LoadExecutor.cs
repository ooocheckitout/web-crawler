namespace common;

public class LoadExecutor
{
    readonly FileWriter _fileWriter;
    readonly SeleniumDownloader _downloader;

    public LoadExecutor(FileWriter fileWriter, SeleniumDownloader downloader)
    {
        _fileWriter = fileWriter;
        _downloader = downloader;
    }

    public async Task LoadContentAsync(string url, string htmlLocation, CancellationToken cancellationToken)
    {
        if (File.Exists(htmlLocation))
            return;

        string htmlContent = await _downloader.DownloadAsTextAsync(url, cancellationToken);
        await _fileWriter.AsTextAsync(htmlLocation, htmlContent, cancellationToken);
    }
}
