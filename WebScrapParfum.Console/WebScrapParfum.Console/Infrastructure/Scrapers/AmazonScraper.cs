using System.Globalization;
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
            _wait.Until(d => LerPreco(d) > 0 || Indisponivel(d));

            var preco = LerPreco(_driver);
            return new ScrapedResult(config, preco, preco > 0);
        }
        catch (WebDriverTimeoutException)
        {
            return new ScrapedResult(config, 0, false);
        }
    }

    // No layout atual da Amazon o <span class="a-offscreen"> do preço "a pagar"
    // vem vazio; o valor vive no rótulo de acessibilidade ou nos spans
    // whole/fraction (aria-hidden). Lê-se via textContent, pois innerText de
    // elementos ocultos (a-offscreen) retorna vazio.
    private static decimal LerPreco(ISearchContext ctx)
    {
        var rotulo = ctx.FindElements(By.CssSelector(
                "#apex-pricetopay-accessibility-label, .apex-pricetopay-accessibility-label"))
            .Select(TextoDe)
            .FirstOrDefault(t => t.Contains("R$"));
        if (rotulo is not null && TryParsePrice(rotulo) is var pr && pr > 0)
            return pr;

        var inteira = ctx.FindElements(By.CssSelector(".priceToPay .a-price-whole")).Select(TextoDe).FirstOrDefault();
        var centavos = ctx.FindElements(By.CssSelector(".priceToPay .a-price-fraction")).Select(TextoDe).FirstOrDefault();
        var composto = ComporPreco(inteira, centavos);
        if (composto > 0) return composto;

        var offscreen = ctx.FindElements(By.CssSelector(".priceToPay .a-offscreen, .apexPriceToPay .a-offscreen"))
            .Select(TextoDe)
            .FirstOrDefault(t => t.Contains("R$"));
        if (offscreen is not null && TryParsePrice(offscreen) is var po && po > 0)
            return po;

        return 0;
    }

    private static decimal ComporPreco(string? inteira, string? centavos)
    {
        if (string.IsNullOrWhiteSpace(inteira) || string.IsNullOrWhiteSpace(centavos))
            return 0;

        var digInteira = new string(inteira.Where(char.IsDigit).ToArray());
        var digCentavos = new string(centavos.Where(char.IsDigit).ToArray());
        if (digInteira.Length == 0 || digCentavos.Length == 0)
            return 0;

        return decimal.TryParse($"{digInteira}.{digCentavos}",
            NumberStyles.Number, CultureInfo.InvariantCulture, out var preco) ? preco : 0;
    }

    private static bool Indisponivel(ISearchContext ctx)
        => ctx.FindElements(By.Id("outOfStock")).Count > 0;

    private static string TextoDe(IWebElement e)
    {
        try { return e.GetAttribute("textContent")?.Trim() ?? string.Empty; }
        catch (StaleElementReferenceException) { return string.Empty; }
    }

    private static decimal TryParsePrice(string text)
    {
        try { return ParsePrice(text); }
        catch { return 0; }
    }
}
