using Microsoft.EntityFrameworkCore;

namespace WebScrapParfum.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    private readonly string _dbPath;

    public AppDbContext(string dbPath) => _dbPath = dbPath;

    public DbSet<PerfumeEntity> Perfumes => Set<PerfumeEntity>();
    public DbSet<PrecoRegistro> PrecoRegistros => Set<PrecoRegistro>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
        => options.UseSqlite($"Data Source={_dbPath}");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PerfumeEntity>(e =>
        {
            e.HasKey(x => x.Id);
            e.Property(x => x.Nome).IsRequired();
            e.Property(x => x.Url).IsRequired();
            e.HasIndex(x => x.Url).IsUnique();
        });

        modelBuilder.Entity<PrecoRegistro>(e =>
        {
            e.HasKey(x => x.Id);
            e.HasOne(x => x.Perfume)
             .WithMany(p => p.Historico)
             .HasForeignKey(x => x.PerfumeId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(x => new { x.PerfumeId, x.CapturadoEm });

            // SQLite não tem tipo decimal: o EF o armazenaria como TEXT, e aí
            // MAX/comparações ficam lexicográficos ("89.8" > "169.9"). Guardamos
            // em centavos (INTEGER) para ordenação/comparação numéricas corretas.
            e.Property(x => x.Preco)
             .HasConversion(
                 valor => (long)Math.Round(valor * 100m, MidpointRounding.AwayFromZero),
                 centavos => centavos / 100m);
        });
    }
}
