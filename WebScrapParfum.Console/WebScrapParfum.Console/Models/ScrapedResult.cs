namespace WebScrapParfum.Models;

public record ScrapedResult(PerfumeConfig Info, decimal PrecoAtual, bool EstaDisponivel = true)
{
    public decimal ValorDesconto => Info.PrecoBase - PrecoAtual;
    public bool TemDesconto => EstaDisponivel && PrecoAtual < Info.PrecoBase;
}
