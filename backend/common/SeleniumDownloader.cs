using OpenQA.Selenium.Chrome;

namespace crawler.tests;

public class SeleniumDownloader : IDisposable
{
    private readonly ChromeDriver _browser;

    public SeleniumDownloader()
    {
        var chromeOptions = new ChromeOptions();
        chromeOptions.AddArguments("headless");
        
        _browser = new ChromeDriver(ChromeDriverService.CreateDefaultService(), chromeOptions, TimeSpan.FromSeconds(10));
    }

    public Task<string> DownloadAsTextAsync(string url, CancellationToken cancellationToken)
    {
        _browser.Navigate().GoToUrl(url);
        return Task.FromResult(_browser.PageSource);
    }

    public void Dispose()
    {
        _browser.Dispose();
    }
}
