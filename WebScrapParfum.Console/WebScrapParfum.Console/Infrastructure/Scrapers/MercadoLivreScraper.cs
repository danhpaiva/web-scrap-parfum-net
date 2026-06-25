using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class MercadoLivreScraper : ScraperBase
{
    public MercadoLivreScraper()
        : base(CreateBaseOptions(disableBlinkAutomation: true, excludeEnableAutomation: true), TimeSpan.FromSeconds(20)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d =>
            {
                var candidates = d.FindElements(By.CssSelector(
                    ".andes-money-amount__fraction, " +
                    "[class*='price-tag-fraction'], " +
                    ".ui-pdp-price__second-line .andes-money-amount, " +
                    ".ui-pdp-price .andes-money-amount__fraction"));

                return candidates.FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text));
            });

            var fraction = element.Text.Trim();
            var cents = _driver
                .FindElements(By.CssSelector(".andes-money-amount__cents"))
                .FirstOrDefault(e => e.Displayed)?.Text.Trim() ?? "00";

            decimal preco = ParsePrice($"{fraction},{cents}");
            return new ScrapedResult(config, preco, true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Sem estoque") ||
                            _driver.PageSource.Contains("Produto indisponível");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
