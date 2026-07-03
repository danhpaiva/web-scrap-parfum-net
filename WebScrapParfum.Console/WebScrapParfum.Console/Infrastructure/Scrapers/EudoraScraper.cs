using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class EudoraScraper : ScraperBase
{
    public EudoraScraper(ILogger<EudoraScraper> logger)
        : base(new DriverSettings(DisableBlinkAutomation: true, ExcludeEnableAutomation: true), TimeSpan.FromSeconds(20), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d =>
            {
                var candidates = d.FindElements(By.CssSelector(
                    "[class*='nproduct-price-value'], " +
                    "[data-testid='price-value'], " +
                    "[class*='sellingPrice'], " +
                    "[class*='price__value'], " +
                    "[class*='sales-price'], " +
                    "[class*='product-price'], " +
                    ".product__price"));

                return candidates.FirstOrDefault(e => e.Displayed && (e.GetAttribute("content") != null || e.Text.Contains("R$")));
            });

            var content = element.GetAttribute("content");
            decimal preco = !string.IsNullOrEmpty(content)
                ? decimal.Parse(content, CultureInfo.InvariantCulture)
                : ParsePrice(element.Text);

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
