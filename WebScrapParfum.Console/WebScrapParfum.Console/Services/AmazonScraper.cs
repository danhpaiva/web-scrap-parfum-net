using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Interface;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class AmazonScraper : IScraper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public AmazonScraper()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(20));
    }

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            // 1. Check de Estoque
            var disponivel = _driver.FindElements(By.Id("add-to-cart-button")).Any(e => e.Displayed);
            if (!disponivel) return new ScrapedResult(config, 0, false);

            // 2. Seletor focado no preço final com desconto (Apex Price)
            var element = _wait.Until(d => {
                // Tentamos primeiro o container que contém o preço final com desconto
                var el = d.FindElements(By.CssSelector("#corePrice_desktop .priceToPay span.a-offscreen, #corePriceDisplay_desktop_feature_div .priceToPay span.a-offscreen"))
                          .FirstOrDefault(e => !string.IsNullOrEmpty(e.GetAttribute("innerText")));

                return el;
            });

            // Pegamos o innerText que contém o valor completo formatado
            string textoPreco = element.GetAttribute("innerText");
            decimal precoCalculado = ParsePrice(textoPreco);

            var todosOsPrecos = _driver.FindElements(By.CssSelector(".priceToPay .a-offscreen, .apexPriceToPay .a-offscreen"))
                                       .Select(e => ParsePrice(e.GetAttribute("innerText")))
                                       .Where(p => p > 0)
                                       .ToList();

            decimal menorPreco = todosOsPrecos.Any() ? todosOsPrecos.Min() : 0;
            return new ScrapedResult(config, menorPreco, menorPreco > 0);
        }
        catch (Exception)
        {
            // Fallback: Tenta pegar o preço de qualquer lugar que tenha a classe a-offscreen
            try
            {
                var fallback = _driver.FindElement(By.CssSelector(".a-price .a-offscreen")).GetAttribute("innerText");
                return new ScrapedResult(config, ParsePrice(fallback), true);
            }
            catch
            {
                return new ScrapedResult(config, 0, false);
            }
        }
    }

    private decimal ParsePrice(string text)
    {
        // Remove quebras de linha e espaços extras
        string textoLimpo = text.Replace("\r", "").Replace("\n", "").Replace(" ", "");

        // Captura o padrão: números, vírgula opcional, e dois números finais
        var match = System.Text.RegularExpressions.Regex.Match(textoLimpo, @"(\d+),(\d{2})");

        if (match.Success)
        {
            // Monta a string no formato "78,00"
            string valorFormatado = $"{match.Groups[1].Value},{match.Groups[2].Value}";
            return decimal.Parse(valorFormatado, new CultureInfo("pt-BR"));
        }

        throw new Exception($"Texto bruto capturado: {text}");
    }

    public void Dispose()
    {
        _driver.Quit();
        _driver.Dispose();
    }
}