using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Interface;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class InTheBoxScraper : IScraper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public InTheBoxScraper()
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

        // 1. Verificação de estoque (Check de Disponibilidade)
        // Na In The Box, geralmente aparece um botão "Avise-me" ou classe 'unavailable'
        var esgotado = _driver.FindElements(By.CssSelector(".p-unavailable, .notify-me, [value='Avise-me']")).Any(e => e.Displayed);

        if (esgotado)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[AVISO] {config.Nome} está esgotado no momento.");
            Console.ResetColor();

            // Retornamos o preço base para não disparar alerta de promoção (ou 0 se preferir)
            return new ScrapedResult(config, config.PrecoBase);
        }

        try
        {
            var element = _wait.Until(d => {
                var el = d.FindElement(By.CssSelector("span.cmp-price-price"));
                return (el.Displayed && el.Text.Contains("R$")) ? el : null;
            });

            decimal precoCalculado = ParsePrice(element.Text);
            return new ScrapedResult(config, precoCalculado);
        }
        catch (WebDriverTimeoutException)
        {
            throw new Exception("Elemento de preço não encontrado (o site pode ter mudado o layout).");
        }
    }

    private decimal ParsePrice(string text)
    {
        string valorLimpo = new string(text.Where(c => char.IsDigit(c) || c == ',').ToArray());
        return decimal.Parse(valorLimpo, new CultureInfo("pt-BR"));
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}