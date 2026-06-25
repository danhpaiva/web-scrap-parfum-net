using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;

namespace WebScrapParfum.Infrastructure.Factories;

public static class WebDriverFactory
{
    private const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    public static IWebDriver Create(DriverSettings settings)
    {
        return TryChrome(settings)
            ?? TryEdge(settings)
            ?? TryFirefox(settings)
            ?? throw new InvalidOperationException(
                "Nenhum navegador compatível encontrado. Instale Chrome, Edge ou Firefox.");
    }

    private static IWebDriver? TryChrome(DriverSettings settings)
    {
        try
        {
            var options = new ChromeOptions();
            ApplyChromiumArgs(options, settings);
            return new ChromeDriver(options);
        }
        catch { return null; }
    }

    private static IWebDriver? TryEdge(DriverSettings settings)
    {
        try
        {
            var options = new EdgeOptions();
            ApplyChromiumArgs(options, settings);
            return new EdgeDriver(options);
        }
        catch { return null; }
    }

    private static IWebDriver? TryFirefox(DriverSettings settings)
    {
        try
        {
            var options = new FirefoxOptions();
            options.AddArgument("--headless");

            if (settings.AddUserAgent)
                options.SetPreference("general.useragent.override", UserAgent);

            // Firefox não suporta os flags de automação do Chromium — ignorados intencionalmente
            return new FirefoxDriver(options);
        }
        catch { return null; }
    }

    private static void ApplyChromiumArgs(ChromiumOptions options, DriverSettings settings)
    {
        options.AddArguments("--headless", "--no-sandbox", "--disable-dev-shm-usage");

        if (settings.AddUserAgent)
            options.AddArgument($"--user-agent={UserAgent}");

        if (settings.DisableBlinkAutomation)
            options.AddArgument("--disable-blink-features=AutomationControlled");

        if (settings.ExcludeEnableAutomation)
            options.AddExcludedArgument("enable-automation");
    }
}
