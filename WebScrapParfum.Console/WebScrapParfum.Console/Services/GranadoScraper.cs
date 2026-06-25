using OpenQA.Selenium;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class GranadoScraper : ScraperBase
{
    public GranadoScraper()
        : base(CreateBaseOptions(addUserAgent: false), TimeSpan.FromSeconds(5)) { }

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
