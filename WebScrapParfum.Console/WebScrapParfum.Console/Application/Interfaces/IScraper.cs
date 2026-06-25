using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Application.Interfaces;

public interface IScraper : IDisposable
{
    ScrapedResult Monitorar(PerfumeConfig config);
}
