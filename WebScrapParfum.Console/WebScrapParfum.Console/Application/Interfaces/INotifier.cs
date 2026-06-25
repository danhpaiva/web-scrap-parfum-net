using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Application.Interfaces;

public interface INotifier
{
    void NotifyStarting(int total);
    void NotifyResult(ScrapedResult result);
    void NotifyError(PerfumeConfig config, string message);
}
