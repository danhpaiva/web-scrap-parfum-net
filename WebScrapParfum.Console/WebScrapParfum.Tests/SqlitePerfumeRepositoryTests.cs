using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Persistence;
using WebScrapParfum.Infrastructure.Repositories;

namespace WebScrapParfum.Tests;

public class SqlitePerfumeRepositoryTests : IDisposable
{
    private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.db");

    private SqlitePerfumeRepository Seed(params PerfumeConfig[] seeds)
    {
        DatabaseInitializer.EnsureSeeded(_dbPath, seeds);
        return new SqlitePerfumeRepository(_dbPath);
    }

    [Fact]
    public void GetAll_AposSeed_UsaPrecoBaseDoSeedComoReferenciaInicial()
    {
        var repo = Seed(new PerfumeConfig("Sr. N", "https://natura/sr-n", 164.90m));

        var perfume = Assert.Single(repo.GetAll());
        Assert.Equal("Sr. N", perfume.Nome);
        Assert.Equal(164.90m, perfume.PrecoBase);
    }

    [Fact]
    public void EnsureSeeded_ChamadaDuasVezes_NaoDuplicaPerfumePorUrl()
    {
        var seed = new PerfumeConfig("Sr. N", "https://natura/sr-n", 164.90m);
        DatabaseInitializer.EnsureSeeded(_dbPath, [seed]);
        DatabaseInitializer.EnsureSeeded(_dbPath, [seed]);

        var repo = new SqlitePerfumeRepository(_dbPath);
        Assert.Single(repo.GetAll());
    }

    [Fact]
    public void RegistrarLeitura_ComPrecoMaior_ElevaBaseParaMaiorPrecoObservado()
    {
        var config = new PerfumeConfig("Sr. N", "https://natura/sr-n", 164.90m);
        var repo = Seed(config);

        // Preço real do site (169,90) é maior que o seed — vira a nova base.
        repo.RegistrarLeitura(new ScrapedResult(config, 169.90m, EstaDisponivel: true));

        Assert.Equal(169.90m, repo.GetAll().Single().PrecoBase);
    }

    [Fact]
    public void RegistrarLeitura_ComPrecoMenor_MantemBaseNoMaiorObservadoEDetectaDesconto()
    {
        var config = new PerfumeConfig("Sr. N", "https://natura/sr-n", 164.90m);
        var repo = Seed(config);
        repo.RegistrarLeitura(new ScrapedResult(config, 169.90m, EstaDisponivel: true));

        // Queda real de preço: base permanece no topo (169,90) e há desconto.
        repo.RegistrarLeitura(new ScrapedResult(config, 130.00m, EstaDisponivel: true));

        var atualizado = repo.GetAll().Single();
        Assert.Equal(169.90m, atualizado.PrecoBase);

        var leitura = new ScrapedResult(atualizado, 130.00m, EstaDisponivel: true);
        Assert.True(leitura.TemDesconto);
        Assert.Equal(39.90m, leitura.ValorDesconto);
    }

    [Theory]
    // Casos onde o menor preço tem dígito inicial maior — quebrariam com
    // ordenação lexicográfica de decimal-como-texto no SQLite ("75.83" > "107.0").
    [InlineData(107.00, 75.83, 107.00)]
    [InlineData(164.90, 89.80, 164.90)]
    public void RegistrarLeitura_BaseUsaComparacaoNumerica_NaoLexicografica(
        double seedBase, double leituraMenor, double baseEsperada)
    {
        var config = new PerfumeConfig("X", "https://loja/x", (decimal)seedBase);
        var repo = Seed(config);

        repo.RegistrarLeitura(new ScrapedResult(config, (decimal)leituraMenor, EstaDisponivel: true));

        Assert.Equal((decimal)baseEsperada, repo.GetAll().Single().PrecoBase);
    }

    [Fact]
    public void RegistrarLeitura_Esgotado_NaoPoluiBaseComPrecoZero()
    {
        var config = new PerfumeConfig("Sr. N", "https://natura/sr-n", 164.90m);
        var repo = Seed(config);

        repo.RegistrarLeitura(new ScrapedResult(config, 0m, EstaDisponivel: false));

        Assert.Equal(164.90m, repo.GetAll().Single().PrecoBase);
    }

    [Fact]
    public void RegistrarLeitura_UrlDesconhecida_NaoLancaNemCriaPerfume()
    {
        var repo = Seed(new PerfumeConfig("Sr. N", "https://natura/sr-n", 164.90m));

        var outro = new PerfumeConfig("Fantasma", "https://natura/inexistente", 100m);
        repo.RegistrarLeitura(new ScrapedResult(outro, 90m, EstaDisponivel: true));

        Assert.Single(repo.GetAll());
    }

    public void Dispose()
    {
        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
        if (File.Exists(_dbPath))
            File.Delete(_dbPath);
    }
}
