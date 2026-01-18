using System.Text.Json;
using WebScrapParfum.Interface;
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

if (listaPerfumes == null) return;

foreach (var perfume in listaPerfumes)
{
    Console.WriteLine($"[LOG] Verificando: {perfume.Nome}...");

    try
    {
        // Obtemos o scraper correto para o domínio atual
        using IScraper scraper = GetScraper(perfume.Url);

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
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[AVISO] Não foi possível processar {perfume.Nome}: {ex.Message}");
        Console.ResetColor();
    }

    Console.WriteLine(new string('-', 30));
}

// Fábrica (Factory Method) para decidir qual estratégia de scraping usar
static IScraper GetScraper(string url)
{
    if (url.Contains("granado.com.br"))
        return new GranadoScraper();

    if (url.Contains("nuancielo.com.br"))
        return new NuancieloScraper();

    if (url.Contains("intheboxperfumes.com.br"))
        return new InTheBoxScraper();

    throw new Exception("Domínio não suportado pelo sistema de scraping.");
}