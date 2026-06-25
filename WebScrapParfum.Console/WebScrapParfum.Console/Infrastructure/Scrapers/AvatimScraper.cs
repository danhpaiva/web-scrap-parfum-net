using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class AvatimScraper : ScraperBase
{
    public AvatimScraper(ILogger<AvatimScraper> logger)
        : base(new DriverSettings(), TimeSpan.FromSeconds(15), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var indisponivel = _driver.FindElements(By.CssSelector(".p-unavailable, .btn-notify, .notify-me"))
                                      .Any(e => e.Displayed);

            if (indisponivel)
                return new ScrapedResult(config, 0, false);

            var element = _wait.Until(d =>
            {
                var el = d.FindElement(By.CssSelector("h2.cmp-price-price"));
                return (el.Displayed && el.Text.Contains("R$")) ? el : null;
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (WebDriverTimeoutException)
        {
            return new ScrapedResult(config, 0, false);
        }
    }
}
