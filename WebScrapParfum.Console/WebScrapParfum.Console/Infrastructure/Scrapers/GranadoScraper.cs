using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class GranadoScraper : ScraperBase
{
    public GranadoScraper(ILogger<GranadoScraper> logger)
        : base(new DriverSettings(AddUserAgent: false), TimeSpan.FromSeconds(5), logger) { }

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        var element = _wait.Until(d =>
        {
            var el = d.FindElement(By.XPath("//span[contains(., 'R$')]"));
            return el.Displayed ? el : null;
        });

        return new ScrapedResult(config, ParsePrice(element.Text));
    }
}
