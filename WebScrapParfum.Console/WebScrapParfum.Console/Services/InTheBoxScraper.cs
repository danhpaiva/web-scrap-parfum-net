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
        options.AddExcludedArgument("enable-automation");
        options.AddArgument("--user-agent=Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36");

        _driver = new ChromeDriver(options);
        _wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(5));
    }

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            // Técnica Sênior: Extrair preço do DataLayer (JavaScript)
            // Isso evita erros de layout visual e problemas com botões que demoram a carregar
            IJavaScriptExecutor js = (IJavaScriptExecutor)_driver;
            var precoViaJs = js.ExecuteScript("return dataLayer[0].ecommerce.items[0].price;");

            if (precoViaJs != null)
            {
                decimal preco = Convert.ToDecimal(precoViaJs, new CultureInfo("en-US"));
                return new ScrapedResult(config, preco, true);
            }

            // Caso o DataLayer falhe, tentamos o seletor visual
            var element = _wait.Until(d => {
                var el = d.FindElement(By.CssSelector("span.cmp-price-price"));
                return (el.Displayed && el.Text.Contains("R$")) ? el : null;
            });

            return new ScrapedResult(config, ParsePrice(element.Text), true);
        }
        catch (Exception)
        {
            // Se tudo falhar, verificamos se há indícios reais de esgotado no texto da página
            bool textoEsgotado = _driver.PageSource.Contains("Esgotado") || _driver.PageSource.Contains("Avise-me");

            if (textoEsgotado)
                return new ScrapedResult(config, 0, false);

            throw new Exception("Não foi possível capturar o preço ou o estado do estoque.");
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