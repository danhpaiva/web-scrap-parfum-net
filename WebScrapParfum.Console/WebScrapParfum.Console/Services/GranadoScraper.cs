using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Globalization;
using WebScrapParfum.Interface;
using WebScrapParfum.Models;

namespace WebScrapParfum.Services;

public class GranadoScraper : IDisposable, IScraper
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
        _driver.Navigate().GoToUrl(config.Url);

        // Seletor mais abrangente para pegar o span de preço pelo valor monetário
        var element = _wait.Until(d => {
            // Tenta encontrar o span que contém "R$" ou o atributo de preço
            var el = d.FindElement(By.XPath("//span[contains(., 'R$')]"));
            return (el.Displayed) ? el : null;
        });

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