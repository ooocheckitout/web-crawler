using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;

namespace common;

public class SeleniumDownloader : IDisposable
{
    readonly AppOptions _options;
    readonly ChromeDriver _browser;

    public SeleniumDownloader(AppOptions options, ILogger<SeleniumDownloader> logger)
    {
        _options = options;
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        chromeOptions.AddArgument("no-sandbox");

        var service = ChromeDriverService.CreateDefaultService();
        // service.HideCommandPromptWindow = true;

        logger.LogInformation("Starting chrome driver");
        _browser = new ChromeDriver(service, chromeOptions);
    }

    public async Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        _browser.Navigate().GoToUrl(url);
        if (url.Contains('#'))
            await Task.Delay(_options.SeleniumPageLoadDelay, cancellationToken);
        return _browser.PageSource;
    }

    public void Dispose() => _browser.Dispose();
}
