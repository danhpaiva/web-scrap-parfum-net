using WebScrapParfum.Models;

namespace WebScrapParfum.Interface;

public interface IScraper : IDisposable
{
    ScrapedResult Monitorar(PerfumeConfig config);
}
