using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
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
                    "#price_display, " +
                    "[data-product-price], " +
                    "[class*='price'], " +
                    "[class*='preco'], " +
                    ".product-price, " +
                    "[data-testid='price']"));

                return candidates.FirstOrDefault(e => e.Displayed &&
                    (e.GetAttribute("content") != null || e.GetAttribute("data-product-price") != null || e.Text.Contains("R$")));
            });

            var content = element.GetAttribute("content");
            var cents = element.GetAttribute("data-product-price");

            decimal preco;
            if (!string.IsNullOrEmpty(content))
                preco = decimal.Parse(content, CultureInfo.InvariantCulture);
            else if (!string.IsNullOrEmpty(cents))
                preco = decimal.Parse(cents, CultureInfo.InvariantCulture) / 100m;
            else
                preco = ParsePrice(element.Text);

            return new ScrapedResult(config, preco, true);
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
