using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Presentation.Notifiers;

public class CompositeNotifier : INotifier
{
    private readonly IReadOnlyList<INotifier> _notifiers;

    public CompositeNotifier(params INotifier[] notifiers)
    {
        _notifiers = notifiers;
    }

    public void NotifyStarting(int total)
    {
        foreach (var n in _notifiers) n.NotifyStarting(total);
    }

    public void NotifyResult(ScrapedResult result)
    {
        foreach (var n in _notifiers) n.NotifyResult(result);
    }

    public void NotifyError(PerfumeConfig config, string message)
    {
        foreach (var n in _notifiers) n.NotifyError(config, message);
    }
}
