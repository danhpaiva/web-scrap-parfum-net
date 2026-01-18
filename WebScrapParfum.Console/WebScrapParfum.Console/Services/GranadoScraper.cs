using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class GranadoScraper : IDisposable
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public GranadoScraper()
    {
        var options = new ChromeOptions();

        // Argumentos para rodar em servidores e evitar bloqueios
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
    }

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        try
        {
            _driver.Navigate().GoToUrl(config.Url);

            var element = _wait.Until(d => {
                var el = d.FindElement(By.XPath("//span[contains(text(), 'R$')]"));
                return (el.Displayed && !string.IsNullOrEmpty(el.Text)) ? el : null;
            });

            decimal precoCalculado = ParsePrice(element.Text);
            return new ScrapedResult(config, precoCalculado);
        }
        catch (WebDriverTimeoutException)
        {
            throw new Exception($"Erro: O preço do perfume '{config.Nome}' não foi encontrado na página. Verifique se a URL está correta ou se o site mudou o layout.");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao processar o perfume {config.Nome}: {ex.Message}");
        }
    }

    private decimal ParsePrice(string text)
    {
        string valorLimpo = text
            .Replace("R$", "")
            .Replace("\u00A0", "")
            .Replace(" ", "")
            .Trim();

        return decimal.Parse(valorLimpo, new CultureInfo("pt-BR"));
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}