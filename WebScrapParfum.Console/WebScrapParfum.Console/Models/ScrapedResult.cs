namespace WebScrapParfum.Models;

public record ScrapedResult(PerfumeConfig Info, decimal PrecoAtual)
{
    public bool TemDesconto => PrecoAtual < Info.PrecoBase;
    public decimal ValorDesconto => Info.PrecoBase - PrecoAtual;
}
