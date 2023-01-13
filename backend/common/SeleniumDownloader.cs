using OpenQA.Selenium.Chrome;

namespace common;

public class SeleniumDownloader
{
    readonly TimeSpan _afterLoadDelay = TimeSpan.FromSeconds(3);

    public async Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        chromeOptions.AddArgument("no-sandbox");

        using var browser = new ChromeDriver(chromeOptions);
        browser.Navigate().GoToUrl(url);
        await Task.Delay(_afterLoadDelay, cancellationToken);
        return browser.PageSource;
    }
}
