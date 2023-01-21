using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;

namespace common;

public class SeleniumDownloader : IDisposable
{
    readonly AppOptions _options;
    readonly ILogger<SeleniumDownloader> _logger;
    readonly ChromeDriver _browser;

    public SeleniumDownloader(AppOptions options, ILogger<SeleniumDownloader> logger)
    {
        _options = options;
        _logger = logger;
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        chromeOptions.AddArgument("no-sandbox");
        chromeOptions.AddUserProfilePreference("profile.default_content_setting_values.images", 2);

        var service = ChromeDriverService.CreateDefaultService();
        service.HideCommandPromptWindow = true;

        _logger.LogInformation("Start initializing chrome driver");
        _browser = new ChromeDriver(service, chromeOptions);
        _logger.LogInformation("Finish initializing chrome driver");
    }

    public async Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Start downloading from {url}", url);

        _browser.Navigate().GoToUrl(url);

        if (url.Contains('#'))
            await Task.Delay(_options.SeleniumPageLoadDelay, cancellationToken);

        _logger.LogDebug("Finis downloading from {url}", url);
        return _browser.PageSource;
    }

    public void Dispose() => _browser.Dispose();
}
