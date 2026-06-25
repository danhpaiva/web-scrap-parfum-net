using WebScrapParfum.Domain.Entities;

namespace WebScrapParfum.Application.Interfaces;

public interface IPerfumeRepository
{
    IReadOnlyList<PerfumeConfig> GetAll();
}
