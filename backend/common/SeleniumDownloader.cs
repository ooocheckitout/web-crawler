using OpenQA.Selenium.Chrome;

namespace crawler.tests;

public class SeleniumDownloader
{
    public Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");

        using var browser = new ChromeDriver(chromeOptions);
        browser.Navigate().GoToUrl(url);
        return Task.FromResult(browser.PageSource);
    }
}
