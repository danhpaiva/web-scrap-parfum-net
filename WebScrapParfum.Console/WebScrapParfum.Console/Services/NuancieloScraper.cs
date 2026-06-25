using OpenQA.Selenium;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class NuancieloScraper : ScraperBase
{
    public NuancieloScraper()
        : base(CreateBaseOptions(disableBlinkAutomation: true, excludeEnableAutomation: true), TimeSpan.FromSeconds(7)) { }

    public override ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        ((OpenQA.Selenium.IJavaScriptExecutor)_driver).ExecuteScript("window.scrollTo(0, 500);");

        var element = _wait.Until(d =>
        {
            var elements = d.FindElements(By.CssSelector("span.total"));
            return elements.FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
        });

        if (element == null)
            throw new InvalidOperationException("Preço não encontrado após o tempo limite.");

        return new ScrapedResult(config, ParsePrice(element.Text));
    }
}
