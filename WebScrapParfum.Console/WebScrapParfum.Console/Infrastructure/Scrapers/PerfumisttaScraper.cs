using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class PerfumisttaScraper : ScraperBase
{
    public PerfumisttaScraper(ILogger<PerfumisttaScraper> logger)
        : base(new DriverSettings(DisableBlinkAutomation: true, ExcludeEnableAutomation: true), TimeSpan.FromSeconds(15), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d =>
            {
                var candidates = d.FindElements(By.CssSelector(
                    ".price-item--sale, " +
                    ".price-item--regular, " +
                    ".price__regular .price-item, " +
                    ".price-item, " +
                    "[class*='price']"));

                return candidates.FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Esgotado") ||
                            _driver.PageSource.Contains("Indisponível") ||
                            _driver.PageSource.Contains("Sold out") ||
                            _driver.PageSource.Contains("Vendido");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
