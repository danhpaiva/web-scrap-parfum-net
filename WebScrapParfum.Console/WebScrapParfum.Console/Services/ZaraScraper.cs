using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class ZaraScraper : ScraperBase
{
    public ZaraScraper()
        : base(CreateBaseOptions(disableBlinkAutomation: true, excludeEnableAutomation: true), TimeSpan.FromSeconds(20)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            // Zara carrega o preço via React — aguarda qualquer seletor de preço visível
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
            // Produto pode estar indisponível ou fora de estoque
            bool esgotado = _driver.PageSource.Contains("Esgotado") ||
                            _driver.PageSource.Contains("Avise-me") ||
                            _driver.PageSource.Contains("out-of-stock");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
