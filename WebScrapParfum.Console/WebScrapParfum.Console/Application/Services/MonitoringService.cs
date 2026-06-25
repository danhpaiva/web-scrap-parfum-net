using System.Collections.Concurrent;
using WebScrapParfum.Application.Interfaces;

namespace WebScrapParfum.Application.Services;

public class MonitoringService
{
    private readonly IPerfumeRepository _repository;
    private readonly IScraperFactory _factory;
    private readonly INotifier _notifier;

    public MonitoringService(IPerfumeRepository repository, IScraperFactory factory, INotifier notifier)
    {
        _repository = repository;
        _factory = factory;
        _notifier = notifier;
    }

    public void Run()
    {
        var perfumes = _repository.GetAll();

        _notifier.NotifyStarting(perfumes.Count);

        var resultados = new ConcurrentBag<(int Ordem, Domain.Entities.PerfumeConfig Perfume, Domain.ValueObjects.ScrapedResult? Resultado, string? Erro)>();

        var options = new ParallelOptions { MaxDegreeOfParallelism = 3 };

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
                resultados.Add((item.Ordem, item.Perfume, null, ex.Message));
            }
        });

        foreach (var (_, perfume, resultado, erro) in resultados.OrderBy(r => r.Ordem))
        {
            if (erro is not null)
                _notifier.NotifyError(perfume, erro);
            else
                _notifier.NotifyResult(resultado!);
        }
    }
}
