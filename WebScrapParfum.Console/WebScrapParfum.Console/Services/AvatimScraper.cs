using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Interface;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class AvatimScraper : IScraper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public AvatimScraper()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(15));
    }

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var indisponivel = _driver.FindElements(By.CssSelector(".p-unavailable, .btn-notify, .notify-me")).Any(e => e.Displayed);

            if (indisponivel)
                return new ScrapedResult(config, 0, false);

            var element = _wait.Until(d => {
                var el = d.FindElement(By.CssSelector("h2.cmp-price-price"));
                return (el.Displayed && el.Text.Contains("R$")) ? el : null;
            });

            decimal precoCalculado = ParsePrice(element.Text);
            return new ScrapedResult(config, precoCalculado, true);
        }
        catch (WebDriverTimeoutException)
        {
            return new ScrapedResult(config, 0, false);
        }
    }

    private decimal ParsePrice(string text)
    {
        string valorLimpo = text.Replace("\u00A0", " ");
        var match = System.Text.RegularExpressions.Regex.Match(valorLimpo, @"\d+,\d{2}");

        if (match.Success)
            return decimal.Parse(match.Value, new CultureInfo("pt-BR"));

        throw new Exception("Formato de preço da Avatim não reconhecido.");
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}