using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class NaturaScraper : ScraperBase
{
    public NaturaScraper()
        : base(CreateBaseOptions(disableBlinkAutomation: true), TimeSpan.FromSeconds(20)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d =>
            {
                var el = d.FindElements(By.CssSelector("[data-testid='price-value'], #product-price"))
                          .FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
                return el;
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Produto indisponível") ||
                            _driver.PageSource.Contains("Avise-me");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
