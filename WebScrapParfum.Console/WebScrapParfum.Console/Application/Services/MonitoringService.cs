using System.Collections.Concurrent;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
using WebScrapParfum.Application.Interfaces;

namespace WebScrapParfum.Application.Services;

public class MonitoringService
{
    private readonly IPerfumeRepository _repository;
    private readonly IScraperFactory _factory;
    private readonly INotifier _notifier;
    private readonly ILogger<MonitoringService> _logger;

    public MonitoringService(
        IPerfumeRepository repository,
        IScraperFactory factory,
        INotifier notifier,
        ILogger<MonitoringService> logger)
    {
        _repository = repository;
        _factory = factory;
        _notifier = notifier;
        _logger = logger;
    }

    public void Run()
    {
        var perfumes = _repository.GetAll();

        _logger.LogInformation("Monitoramento iniciado — {Total} perfume(s) em paralelo", perfumes.Count);
        _notifier.NotifyStarting(perfumes.Count);

        var resultados = new ConcurrentBag<(int Ordem, Domain.Entities.PerfumeConfig Perfume, Domain.ValueObjects.ScrapedResult? Resultado, string? Erro)>();
        var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };
        var sw = Stopwatch.StartNew();

        Parallel.ForEach(perfumes.Select((p, i) => (Perfume: p, Ordem: i)), options, item =>
        {
            try
            {
                using var scraper = _factory.Create(item.Perfume.Url);
                var resultado = scraper.Monitorar(item.Perfume);
                resultados.Add((item.Ordem, item.Perfume, resultado, null));
            }
            catch (Exception ex)
            {
                _logger.LogError("Falha ao processar {Nome}: {Mensagem}", item.Perfume.Nome, ex.Message);
                resultados.Add((item.Ordem, item.Perfume, null, ex.Message));
            }
        });

        sw.Stop();

        var ordenados = resultados.OrderBy(r => r.Ordem).ToList();
        int promocoes = ordenados.Count(r => r.Resultado?.TemDesconto == true);
        int erros = ordenados.Count(r => r.Erro is not null);

        foreach (var (_, perfume, resultado, erro) in ordenados)
        {
            if (erro is not null)
                _notifier.NotifyError(perfume, erro);
            else
                _notifier.NotifyResult(resultado!);
        }

        _logger.LogInformation(
            "Monitoramento concluído em {Elapsed}s — {Promocoes} promoção(ões), {Erros} erro(s)",
            sw.Elapsed.TotalSeconds.ToString("F1"), promocoes, erros);
    }
}
