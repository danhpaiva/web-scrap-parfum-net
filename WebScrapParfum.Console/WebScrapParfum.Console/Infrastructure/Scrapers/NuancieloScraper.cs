using WebScrapParfum.Infrastructure.Factories;
using OpenQA.Selenium;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class NuancieloScraper : ScraperBase
{
    public NuancieloScraper()
        : base(new DriverSettings(DisableBlinkAutomation: true, ExcludeEnableAutomation: true), TimeSpan.FromSeconds(7)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        ((IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 500);");

        var element = _wait.Until(d =>
        {
            var elements = d.FindElements(By.CssSelector("span.total"));
            return elements.FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
        });

        if (element == null)
            throw new InvalidOperationException("PreÃ§o nÃ£o encontrado apÃ³s o tempo limite.");

        return new ScrapedResult(config, ParsePrice(element.Text));
    }
}
