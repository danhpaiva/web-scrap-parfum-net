using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using System.Text.RegularExpressions;
using WebScrapParfum.Interface;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public abstract class ScraperBase : IScraper
{
    protected readonly IWebDriver _driver;
    protected readonly WebDriverWait _wait;
    private bool _disposed;

    protected ScraperBase(ChromeOptions options, TimeSpan waitTimeout)
    {
        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, waitTimeout);
    }

    public abstract ScrapedResult Monitorar(PerfumeConfig config);

    protected static ChromeOptions CreateBaseOptions(
        bool addUserAgent = true,
        bool disableBlinkAutomation = false,
        bool excludeEnableAutomation = false)
    {
        var options = new ChromeOptions();
        options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");

        if (addUserAgent)
            options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        if (disableBlinkAutomation)
            options.AddArgument("--disable-blink-features=AutomationControlled");

        if (excludeEnableAutomation)
            options.AddExcludedArgument("enable-automation");

        return options;
    }

    protected static decimal ParsePrice(string text)
    {
        string clean = text.Replace(" ", " ").Replace("\r", "").Replace("\n", "").Trim();
        var match = Regex.Match(clean, @"\d+,\d{2}");

        if (match.Success)
            return decimal.Parse(match.Value, new CultureInfo("pt-BR"));

        throw new FormatException($"Não foi possível extrair o preço de: '{text}'");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _driver.Quit();
        _driver.Dispose();
        GC.SuppressFinalize(this);
    }
}
