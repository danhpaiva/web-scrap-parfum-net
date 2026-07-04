using WebScrapParfum.Domain.Entities;

namespace WebScrapParfum.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    // Cria o banco se necessário e importa da lista de seed os perfumes ainda
    // ausentes (casados por URL). O PrecoBase do seed vira a primeira leitura de
    // histórico, servindo como referência inicial de "maior preço observado".
    public static void EnsureSeeded(string dbPath, IEnumerable<PerfumeConfig> seeds)
    {
        using var db = new AppDbContext(dbPath);
        db.Database.EnsureCreated();

        var urlsExistentes = db.Perfumes.Select(p => p.Url).ToHashSet();
        var agora = DateTime.Now;

        foreach (var seed in seeds)
        {
            if (urlsExistentes.Contains(seed.Url)) continue;

            db.Perfumes.Add(new PerfumeEntity
            {
                Nome = seed.Nome,
                Url = seed.Url,
                Historico =
                {
                    new PrecoRegistro
                    {
                        Preco = seed.PrecoBase,
                        Disponivel = true,
                        CapturadoEm = agora
                    }
                }
            });
        }

        db.SaveChanges();
    }
}
