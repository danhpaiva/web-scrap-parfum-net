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
                var el = d.FindElement(By.Id("product-price"));
                return (el.Displayed && el.Text.Contains("R$")) ? el : null;
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
        // Remove espaços inquebráveis (&nbsp;) e limpa o valor
        string valorLimpo = text.Replace("\u00A0", " ");
        valorLimpo = new string(valorLimpo.Where(c => char.IsDigit(c) || c == ',').ToArray());
        return decimal.Parse(valorLimpo, new CultureInfo("pt-BR"));
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}
