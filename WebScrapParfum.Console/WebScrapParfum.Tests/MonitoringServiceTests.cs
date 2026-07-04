using Microsoft.Extensions.Logging.Abstractions;
using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Application.Services;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Tests;

public class MonitoringServiceTests
{
    private sealed class FakeRepository(IReadOnlyList<PerfumeConfig> perfumes) : IPerfumeRepository
    {
        public List<ScrapedResult> LeiturasRegistradas { get; } = [];

        public IReadOnlyList<PerfumeConfig> GetAll() => perfumes;
        public void RegistrarLeitura(ScrapedResult resultado) => LeiturasRegistradas.Add(resultado);
    }

    private sealed class FakeScraper(Func<PerfumeConfig, ScrapedResult> monitorar) : IScraper
    {
        public ScrapedResult Monitorar(PerfumeConfig config) => monitorar(config);
        public void Dispose() { }
    }

    private sealed class ThrowingScraper(string mensagemErro) : IScraper
    {
        public ScrapedResult Monitorar(PerfumeConfig config) => throw new InvalidOperationException(mensagemErro);
        public void Dispose() { }
    }

    private sealed class FakeScraperFactory(Func<string, IScraper> create) : IScraperFactory
    {
        public IScraper Create(string url) => create(url);
    }

    private sealed class FakeNotifier : INotifier
    {
        public int TotalNotificado { get; private set; }
        public List<ScrapedResult> Resultados { get; } = [];
        public List<(PerfumeConfig Perfume, string Mensagem)> Erros { get; } = [];

        public void NotifyStarting(int total) => TotalNotificado = total;
        public void NotifyResult(ScrapedResult result) => Resultados.Add(result);
        public void NotifyError(PerfumeConfig config, string message) => Erros.Add((config, message));
    }

    [Fact]
    public void Run_ComPerfumesValidos_NotificaTodosNaOrdemOriginalENoTotalCorreto()
    {
        var perfumes = new List<PerfumeConfig>
        {
            new("Bossa", "https://exemplo.com/bossa", 195.00m),
            new("Wild", "https://exemplo.com/wild", 169.90m),
            new("Caribbean", "https://exemplo.com/caribbean", 174.90m),
        };

        var factory = new FakeScraperFactory(url =>
            new FakeScraper(config => new ScrapedResult(config, config.PrecoBase - 10m, true)));

        var notifier = new FakeNotifier();
        var service = new MonitoringService(
            new FakeRepository(perfumes), factory, notifier, NullLogger<MonitoringService>.Instance);

        service.Run();

        Assert.Equal(3, notifier.TotalNotificado);
        Assert.Equal(perfumes.Select(p => p.Nome), notifier.Resultados.Select(r => r.Info.Nome));
        Assert.Empty(notifier.Erros);
    }

    [Fact]
    public void Run_QuandoScraperFalha_NotificaErroSemInterromperOsDemais()
    {
        var perfumes = new List<PerfumeConfig>
        {
            new("Bossa", "https://exemplo.com/bossa", 195.00m),
            new("Dominio Quebrado", "https://naosuportado.com/produto", 100.00m),
        };

        var factory = new FakeScraperFactory(url => url.Contains("naosuportado")
            ? throw new NotSupportedException($"Domínio não suportado: {new Uri(url).Host}")
            : new FakeScraper(config => new ScrapedResult(config, config.PrecoBase, true)));

        var notifier = new FakeNotifier();
        var service = new MonitoringService(
            new FakeRepository(perfumes), factory, notifier, NullLogger<MonitoringService>.Instance);

        service.Run();

        Assert.Single(notifier.Resultados);
        Assert.Equal("Bossa", notifier.Resultados[0].Info.Nome);

        Assert.Single(notifier.Erros);
        Assert.Equal("Dominio Quebrado", notifier.Erros[0].Perfume.Nome);
        Assert.Contains("naosuportado.com", notifier.Erros[0].Mensagem);
    }

    [Fact]
    public void Run_QuandoScraperLancaExcecaoAoMonitorar_NotificaErroComMensagemOriginal()
    {
        var perfumes = new List<PerfumeConfig> { new("Revelation", "https://exemplo.com/revelation", 109.90m) };

        var factory = new FakeScraperFactory(_ => new ThrowingScraper("Timeout ao carregar a página"));
        var notifier = new FakeNotifier();
        var service = new MonitoringService(
            new FakeRepository(perfumes), factory, notifier, NullLogger<MonitoringService>.Instance);

        service.Run();

        Assert.Empty(notifier.Resultados);
        Assert.Single(notifier.Erros);
        Assert.Equal("Timeout ao carregar a página", notifier.Erros[0].Mensagem);
    }
}
