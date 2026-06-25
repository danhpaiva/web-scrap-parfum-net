using WebScrapParfum.Application.Services;
using WebScrapParfum.Infrastructure.Factories;
using WebScrapParfum.Infrastructure.Repositories;
using WebScrapParfum.Presentation.Notifiers;

var jsonPath = Path.Combine(AppContext.BaseDirectory, "perfumes.json");

var repository = new JsonPerfumeRepository(jsonPath);
var factory    = new ScraperFactory();
var notifier   = new ConsoleNotifier();

var service = new MonitoringService(repository, factory, notifier);

try
{
    service.Run();
}
catch (FileNotFoundException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Erro fatal: {ex.Message}");
    Console.ResetColor();
}
