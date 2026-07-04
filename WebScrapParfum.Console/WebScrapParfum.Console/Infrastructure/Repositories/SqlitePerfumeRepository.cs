using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Persistence;

namespace WebScrapParfum.Infrastructure.Repositories;

public class SqlitePerfumeRepository : IPerfumeRepository
{
    private readonly string _dbPath;

    public SqlitePerfumeRepository(string dbPath) => _dbPath = dbPath;

    public IReadOnlyList<PerfumeConfig> GetAll()
    {
        using var db = new AppDbContext(_dbPath);

        var perfumes = db.Perfumes
            .Select(p => new { p.Id, p.Nome, p.Url })
            .ToList();

        return perfumes.Select(p =>
        {
            // Base = maior preço já observado (ignora leituras zeradas de esgotado).
            // Agregação no cliente com decimal, garantindo comparação numérica.
            var precoBase = db.PrecoRegistros
                .Where(h => h.PerfumeId == p.Id)
                .Select(h => h.Preco)
                .AsEnumerable()
                .Where(preco => preco > 0)
                .DefaultIfEmpty(0m)
                .Max();

            return new PerfumeConfig(p.Nome, p.Url, precoBase);
        }).ToList();
    }

    public void RegistrarLeitura(ScrapedResult resultado)
    {
        using var db = new AppDbContext(_dbPath);

        var perfumeId = db.Perfumes
            .Where(p => p.Url == resultado.Info.Url)
            .Select(p => (int?)p.Id)
            .FirstOrDefault();

        if (perfumeId is null) return;

        db.PrecoRegistros.Add(new PrecoRegistro
        {
            PerfumeId = perfumeId.Value,
            Preco = resultado.PrecoAtual,
            Disponivel = resultado.EstaDisponivel,
            CapturadoEm = DateTime.Now
        });

        db.SaveChanges();
    }
}
