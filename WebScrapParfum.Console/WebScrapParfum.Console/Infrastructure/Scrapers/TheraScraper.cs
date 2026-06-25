using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class TheraScraper : ScraperBase
{
    public TheraScraper(ILogger<TheraScraper> logger)
        : base(new DriverSettings(), TimeSpan.FromSeconds(10), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d =>
            {
                var candidates = d.FindElements(By.CssSelector(
                    "[class*='price'], " +
                    "[class*='preco'], " +
                    ".product-price, " +
                    "[data-testid='price']"));

                return candidates.FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Esgotado") ||
                            _driver.PageSource.Contains("Indisponível") ||
                            _driver.PageSource.Contains("Avise-me");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
