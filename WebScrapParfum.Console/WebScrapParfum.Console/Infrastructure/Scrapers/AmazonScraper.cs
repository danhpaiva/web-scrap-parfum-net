using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class AmazonScraper : ScraperBase
{
    public AmazonScraper(ILogger<AmazonScraper> logger)
        : base(new DriverSettings(), TimeSpan.FromSeconds(20), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            // A disponibilidade é derivada da presença de um preço de compra
            // (priceToPay). Checar #add-to-cart-button diretamente era frágil:
            // o botão carrega tarde ou muda conforme o buybox, gerando falso
            // "Esgotado" mesmo com o produto à venda.
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
                var preco = ParsePrice(fallbackText);
                return new ScrapedResult(config, preco, preco > 0);
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
