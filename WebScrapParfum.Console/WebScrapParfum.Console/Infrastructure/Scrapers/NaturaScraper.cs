using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class NaturaScraper : ScraperBase
{
    public NaturaScraper(ILogger<NaturaScraper> logger)
        : base(new DriverSettings(DisableBlinkAutomation: true), TimeSpan.FromSeconds(20), logger) { }

    // A página da Natura repete id="product-price" em blocos de produtos
    // relacionados. O primeiro no DOM é um contêiner de recomendação cujo texto
    // concatena vários preços ("R$ 89,80R$ 71,84 -20%"); o preço real do produto
    // é o único cujo texto é um preço único e limpo.
    private static readonly Regex PrecoUnico =
        new(@"^R\$\s*\d{1,3}(?:\.\d{3})*,\d{2}$", RegexOptions.Compiled);

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(_ => EncontrarPrecoPrincipal());
            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Produto indisponível") ||
                            _driver.PageSource.Contains("Avise-me");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }

    private IWebElement? EncontrarPrecoPrincipal()
    {
        var candidatos = _driver.FindElements(
            By.CssSelector("#product-price, [data-testid='price-value']"));

        foreach (var e in candidatos)
        {
            try
            {
                if (e.Displayed && PrecoUnico.IsMatch(e.Text.Trim()))
                    return e;
            }
            catch (StaleElementReferenceException)
            {
                // DOM em hidratação; tenta novamente no próximo poll do wait.
            }
        }

        return null;
    }
}
