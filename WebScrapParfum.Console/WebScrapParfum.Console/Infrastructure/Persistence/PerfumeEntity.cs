namespace WebScrapParfum.Infrastructure.Persistence;

public class PerfumeEntity
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public List<PrecoRegistro> Historico { get; set; } = [];
}
