using WebScrapParfum.Infrastructure.Factories;
using OpenQA.Selenium;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class GranadoScraper : ScraperBase
{
    public GranadoScraper()
        : base(new DriverSettings(AddUserAgent: false), TimeSpan.FromSeconds(5)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
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
