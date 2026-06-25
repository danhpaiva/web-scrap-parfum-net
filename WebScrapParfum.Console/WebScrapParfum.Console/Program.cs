using Microsoft.Extensions.Logging;
using WebScrapParfum.Application.Services;
using WebScrapParfum.Infrastructure.Factories;
using WebScrapParfum.Infrastructure.Repositories;
using WebScrapParfum.Presentation.Notifiers;

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .SetMinimumLevel(LogLevel.Information)
        .AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "HH:mm:ss ";
        });
});

var jsonPath   = Path.Combine(AppContext.BaseDirectory, "perfumes.json");
var desktop    = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
var outputFile = Path.Combine(desktop, $"lista_perfumes_{DateTime.Today:yyyy-MM-dd}.txt");

var repository = new JsonPerfumeRepository(jsonPath);
var factory    = new ScraperFactory(loggerFactory);

using var fileNotifier = new FileNotifier(outputFile);
var notifier = new CompositeNotifier(new ConsoleNotifier(), fileNotifier);

var service = new MonitoringService(
    repository,
    factory,
    notifier,
    loggerFactory.CreateLogger<MonitoringService>());

try
{
    service.Run();
    Console.WriteLine($"[LOG] Resultado salvo em: {outputFile}");
}
catch (FileNotFoundException ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Erro fatal: {ex.Message}");
    Console.ResetColor();
}
