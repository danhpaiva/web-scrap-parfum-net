using WebScrapParfum.Infrastructure.Factories;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class BoticarioScraper : ScraperBase
{
    public BoticarioScraper()
        : base(new DriverSettings(DisableBlinkAutomation: true, ExcludeEnableAutomation: true), TimeSpan.FromSeconds(15)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d =>
            {
                var candidates = d.FindElements(By.CssSelector(
                    "[class*='price__value'], " +
                    "[class*='sales-price'], " +
                    "[data-testid='product-price'], " +
                    ".product__price, " +
                    "[class*='product-price']"));

                return candidates.FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Esgotado") ||
                            _driver.PageSource.Contains("IndisponÃ­vel") ||
                            _driver.PageSource.Contains("Avise-me");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
