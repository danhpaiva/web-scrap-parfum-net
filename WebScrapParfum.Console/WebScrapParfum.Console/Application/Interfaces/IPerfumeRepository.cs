using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Application.Interfaces;

public interface IPerfumeRepository
{
    IReadOnlyList<PerfumeConfig> GetAll();
    void RegistrarLeitura(ScrapedResult resultado);
}
