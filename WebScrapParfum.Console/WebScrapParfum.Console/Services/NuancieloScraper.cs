using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Interface;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class NuancieloScraper : IScraper
{
    private readonly IWebDriver _driver;
    private readonly WebDriverWait _wait;

    public NuancieloScraper()
    {
        var options = new ChromeOptions();
        options.AddArgument("--headless");
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-blink-features=AutomationControlled"); // Oculta que é automação
        options.AddExcludedArgument("enable-automation"); // Remove o banner de "controlado por software"
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(7));
    }

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        // Força um scroll para garantir que os scripts de "lazy load" de preço sejam disparados
        IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
        js.ExecuteScript("window.scrollTo(0, 500);");

        string cssSelector = "span.total";

        var element = _wait.Until(d => {
            try
            {
                // Buscamos todos os elementos com essa classe
                var elements = d.FindElements(By.CssSelector(cssSelector));

                // Procuramos o que pertence ao formulário de compra e tem texto
                var el = elements.FirstOrDefault(e => e.Displayed && e.Text.Contains("R$"));

                return el;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
        });

        if (element == null) throw new Exception("Preço não encontrado após o tempo limite.");

        decimal precoCalculado = ParsePrice(element.Text);
        return new ScrapedResult(config, precoCalculado);
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