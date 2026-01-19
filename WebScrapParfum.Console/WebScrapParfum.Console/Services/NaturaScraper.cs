using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Interface;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class NaturaScraper : IScraper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public NaturaScraper()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-blink-features=AutomationControlled");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
    }

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var element = _wait.Until(d => {
                var el = d.FindElements(By.CssSelector("[data-testid='price-value'], #product-price"))
                          .FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));
                return el;
            });

            decimal precoCalculado = ParsePrice(element.Text);
            return new ScrapedResult(config, precoCalculado, true);
        }
        catch (WebDriverTimeoutException)
        {
            // Verifica se o texto "Produto indisponível" aparece na página
            bool esgotado = _driver.PageSource.Contains("Produto indisponível") ||
                           _driver.PageSource.Contains("Avise-me");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }

    private decimal ParsePrice(string text)
    {
        string textoLimpo = text.Replace("\u00A0", " ").Trim();

        var match = System
            .Text
            .RegularExpressions
            .Regex.Match(textoLimpo, @"\d+,\d{2}");

        if (match.Success)
            return decimal.Parse(match.Value, new CultureInfo("pt-BR"));

        throw new FormatException($"Não foi possível isolar o preço no texto: {text}");
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
