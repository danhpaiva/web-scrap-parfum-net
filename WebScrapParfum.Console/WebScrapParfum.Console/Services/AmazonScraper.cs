using OpenQA.Selenium;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class AmazonScraper : ScraperBase
{
    public AmazonScraper()
        : base(CreateBaseOptions(), TimeSpan.FromSeconds(20)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var disponivel = _driver.FindElements(By.Id("add-to-cart-button")).Any(e => e.Displayed);
            if (!disponivel) return new ScrapedResult(config, 0, false);

            _wait.Until(d =>
                d.FindElements(By.CssSelector(
                    "#corePrice_desktop .priceToPay span.a-offscreen, " +
                    "#corePriceDisplay_desktop_feature_div .priceToPay span.a-offscreen"))
                 .FirstOrDefault(e => !string.IsNullOrEmpty(e.GetAttribute("innerText"))));

            var precos = _driver
                .FindElements(By.CssSelector(".priceToPay .a-offscreen, .apexPriceToPay .a-offscreen"))
                .Select(e => e.GetAttribute("innerText"))
                .Where(t => !string.IsNullOrEmpty(t))
                .Select(t => TryParsePrice(t!))
                .Where(p => p > 0)
                .ToList();

            decimal menorPreco = precos.Any() ? precos.Min() : 0;
            return new ScrapedResult(config, menorPreco, menorPreco > 0);
        }
        catch (Exception)
        {
            try
            {
                var fallbackText = _driver.FindElement(By.CssSelector(".a-price .a-offscreen"))
                                          .GetAttribute("innerText") ?? string.Empty;
                return new ScrapedResult(config, ParsePrice(fallbackText), true);
            }
            catch
            {
                return new ScrapedResult(config, 0, false);
            }
        }
    }

    private static decimal TryParsePrice(string text)
    {
        try { return ParsePrice(text); }
        catch { return 0; }
    }
}
