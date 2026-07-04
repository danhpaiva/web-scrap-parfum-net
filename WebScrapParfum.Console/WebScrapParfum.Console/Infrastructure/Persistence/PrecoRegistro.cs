namespace WebScrapParfum.Infrastructure.Persistence;

public class PrecoRegistro
{
    public int Id { get; set; }
    public int PerfumeId { get; set; }
    public PerfumeEntity Perfume { get; set; } = null!;
    public decimal Preco { get; set; }
    public bool Disponivel { get; set; }
    public DateTime CapturadoEm { get; set; }
}
