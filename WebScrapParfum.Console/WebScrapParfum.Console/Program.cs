using System.Text.Json;
using WebScrapParfum.Models;
using WebScrapParfum.Services;

var jsonPath = Path.Combine(AppContext.BaseDirectory, "perfumes.json");

if (!File.Exists(jsonPath))
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"Erro fatal: Arquivo de configuração não encontrado em: {jsonPath}");
    Console.ResetColor();
    return;
}

var jsonContent = File.ReadAllText(jsonPath);
var listaPerfumes = JsonSerializer.Deserialize<List<PerfumeConfig>>(jsonContent);

using var scraper = new GranadoScraper();

foreach (var perfume in listaPerfumes)
{
    Console.WriteLine($"[LOG] Verificando: {perfume.Nome}...");

    var resultado = scraper.Monitorar(perfume);

    Console.WriteLine($"[LOG] Preço encontrado: {resultado.PrecoAtual:C} (Base: {perfume.PrecoBase:C})");

    if (resultado.TemDesconto)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"!!! PROMOÇÃO DETECTADA !!!");
        Console.WriteLine($"Perfume: {resultado.Info.Nome}");
        Console.WriteLine($"Desconto de: {resultado.ValorDesconto:C}");
        Console.ResetColor();
    }
    else
    {
        Console.WriteLine($"[INFO] Sem desconto relevante para {perfume.Nome}.");
    }
    Console.WriteLine(new string('-', 30));
}