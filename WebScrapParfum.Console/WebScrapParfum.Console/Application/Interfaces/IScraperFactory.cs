namespace WebScrapParfum.Application.Interfaces;

public interface IScraperFactory
{
    IScraper Create(string url);
}
