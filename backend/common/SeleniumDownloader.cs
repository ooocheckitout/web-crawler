using OpenQA.Selenium.Chrome;

namespace common;

public class SeleniumDownloader : IDisposable
{
    readonly TimeSpan _afterLoadDelay = TimeSpan.FromSeconds(3);
    readonly ChromeDriver _browser;

    public SeleniumDownloader()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        chromeOptions.AddArgument("no-sandbox");

        var service = ChromeDriverService.CreateDefaultService();
        // service.HideCommandPromptWindow = true;

        _browser = new ChromeDriver(service, chromeOptions);

    }

    public async Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        _browser.Navigate().GoToUrl(url);
        await Task.Delay(_afterLoadDelay, cancellationToken);
        return _browser.PageSource;
    }

    public void Dispose() => _browser.Dispose();
}
