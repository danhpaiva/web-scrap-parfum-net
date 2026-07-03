using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;
using System.Globalization;
using System.Text.RegularExpressions;
using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public abstract class ScraperBase : IScraper
{
    protected readonly IWebDriver _driver;
    protected readonly WebDriverWait _wait;
    private readonly ILogger _logger;
    private bool _disposed;

    protected ScraperBase(DriverSettings settings, TimeSpan waitTimeout, ILogger logger)
    {
        _logger = logger;
        _driver = WebDriverFactory.Create(settings, logger);
        _wait = new WebDriverWait(_driver, waitTimeout);
    }

    private const int MaxTentativas = 3;
    private static readonly TimeSpan AtrasoEntreRetries = TimeSpan.FromSeconds(5);

    public ScrapedResult Monitorar(PerfumeConfig config)
    {
        _logger.LogInformation("Iniciando scraping: {Nome}", config.Nome);
        var sw = Stopwatch.StartNew();

        for (int tentativa = 1; tentativa <= MaxTentativas; tentativa++)
        {
            try
            {
                var result = Execute(config);
                sw.Stop();
                _logger.LogInformation("Concluído: {Nome} em {Elapsed}ms", config.Nome, sw.ElapsedMilliseconds);
                return result;
            }
            catch (Exception ex) when (tentativa < MaxTentativas)
            {
                _logger.LogWarning(
                    "Tentativa {Tentativa}/{Max} falhou para {Nome}: {Mensagem}. Aguardando {Delay}s...",
                    tentativa, MaxTentativas, config.Nome, ex.Message, AtrasoEntreRetries.TotalSeconds);

                Thread.Sleep(AtrasoEntreRetries);
            }
        }

        // Última tentativa — deixa a exceção propagar
        try
        {
            return Execute(config);
        }
        catch (Exception)
        {
            sw.Stop();
            _logger.LogWarning("Falhou após {Max} tentativas: {Nome} ({Elapsed}ms)", MaxTentativas, config.Nome, sw.ElapsedMilliseconds);
            throw;
        }
    }

    protected abstract ScrapedResult Execute(PerfumeConfig config);

    protected static decimal ParsePrice(string text)
    {
        string clean = text.Replace(" ", " ").Replace("\r", "").Replace("\n", "").Trim();
        var match = Regex.Match(clean, @"\d{1,3}(?:\.\d{3})*,\d{2}");

        if (match.Success)
            return decimal.Parse(match.Value, new CultureInfo("pt-BR"));

        throw new FormatException($"Não foi possível extrair o preço de: '{text}'");
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _driver.Quit();
        _driver.Dispose();
        GC.SuppressFinalize(this);
    }
}
