using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class NaturaScraper : ScraperBase
{
    public NaturaScraper(ILogger<NaturaScraper> logger)
        : base(new DriverSettings(DisableBlinkAutomation: true), TimeSpan.FromSeconds(20), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            // Espera pelo preço canônico do produto. O fallback só é usado se
            // #product-price nunca aparecer, evitando corrida onde o seletor
            // genérico captura um preço secundário (ex.: assinatura).
            var element = _wait.Until(d =>
                d.FindElements(By.CssSelector("#product-price"))
                 .FirstOrDefault(e => e.Displayed && e.Text.Contains("R$")));

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            var fallback = _driver.FindElements(By.CssSelector("[data-testid='price-value']"))
                                  .FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
            if (fallback is not null)
                return new ScrapedResult(config, ParsePrice(fallback.Text), true);

            bool esgotado = _driver.PageSource.Contains("Produto indisponível") ||
                            _driver.PageSource.Contains("Avise-me");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }
}
