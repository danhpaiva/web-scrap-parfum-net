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
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5)); // Aumentamos para sites mais lentos
    }

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        string cssSelector = "#form-add-cart .product-action-price.final .total";

        var element = _wait.Until(d => {
            try
            {
                var el = d.FindElement(By.CssSelector(cssSelector));

                // SÊNIOR TIP: Não basta o elemento existir, ele precisa ter o texto "R$" 
                // porque o AJAX pode carregar o span vazio antes de preencher o preço.
                if (el.Displayed && el.Text.Contains("R$"))
                {
                    return el;
                }
                return null;
            }
            catch (NoSuchElementException)
            {
                return null;
            }
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