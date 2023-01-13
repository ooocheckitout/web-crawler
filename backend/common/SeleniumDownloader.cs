using OpenQA.Selenium.Chrome;

namespace common;

public class SeleniumDownloader
{
    public Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        chromeOptions.AddArgument("no-sandbox");

        using var browser = new ChromeDriver(ChromeDriverService.CreateDefaultService(), chromeOptions, TimeSpan.FromSeconds(30));
        browser.Navigate().GoToUrl(url);
        string? pageSource = browser.PageSource;
        return Task.FromResult(pageSource);
    }
}
