using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class ZaraScraper : ScraperBase
{
    public ZaraScraper(ILogger<ZaraScraper> logger)
        : base(new DriverSettings(DisableBlinkAutomation: true, ExcludeEnableAutomation: true), TimeSpan.FromSeconds(20), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d =>
            {
                var candidates = d.FindElements(By.CssSelector(
                    ".price__amount, " +
                    "[data-qa-qualifier='price'], " +
                    ".money-amount__main, " +
                    "[class*='price-current__amount']"));

                return candidates.FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Esgotado") ||
                            _driver.PageSource.Contains("Avise-me") ||
                            _driver.PageSource.Contains("out-of-stock");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
