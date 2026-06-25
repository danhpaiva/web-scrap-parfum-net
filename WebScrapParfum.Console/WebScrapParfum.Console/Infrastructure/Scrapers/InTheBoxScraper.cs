using OpenQA.Selenium;
using System.Globalization;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class InTheBoxScraper : ScraperBase
{
    public InTheBoxScraper()
        : base(CreateBaseOptions(excludeEnableAutomation: true), TimeSpan.FromSeconds(5)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var js = (IJavaScriptExecutor)_driver;
            var precoViaJs = js.ExecuteScript("return dataLayer[0].ecommerce.items[0].price;");

            if (precoViaJs != null)
            {
                decimal preco = Convert.ToDecimal(precoViaJs, new CultureInfo("en-US"));
                return new ScrapedResult(config, preco, true);
            }

            var element = _wait.Until(d =>
            {
                var el = d.FindElement(By.CssSelector("span.cmp-price-price"));
                return (el.Displayed && el.Text.Contains("R$")) ? el : null;
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (Exception)
        {
            bool esgotado = _driver.PageSource.Contains("Esgotado") || _driver.PageSource.Contains("Avise-me");

            if (esgotado)
                return new ScrapedResult(config, 0, false);

            throw new InvalidOperationException("Não foi possível capturar o preço ou o estado do estoque.");
        }
    }
}
