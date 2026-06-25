using System.Collections.Concurrent;
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

if (listaPerfumes is null or []) return;

Console.WriteLine($"[LOG] Iniciando monitoramento de {listaPerfumes.Count} perfume(s) em paralelo...");
Console.WriteLine(new string('-', 50));

var resultados = new ConcurrentBag<(int Ordem, PerfumeConfig Perfume, ScrapedResult? Resultado, string? Erro)>();

var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 3 };

Parallel.ForEach(listaPerfumes.Select((p, i) => (Perfume: p, Ordem: i)), parallelOptions, item =>
{
    try
    {
        using IScraper scraper = GetScraper(item.Perfume.Url);
        var resultado = scraper.Monitorar(item.Perfume);
        resultados.Add((item.Ordem, item.Perfume, resultado, null));
    }
    catch (Exception ex)
    {
        resultados.Add((item.Ordem, item.Perfume, null, ex.Message));
    }
});

foreach (var (_, perfume, resultado, erro) in resultados.OrderBy(r => r.Ordem))
{
    Console.WriteLine($"[LOG] Verificado: {perfume.Nome}");

    if (erro is not null)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[AVISO] Não foi possível processar {perfume.Nome}: {erro}");
        Console.ResetColor();
    }
    else if (!resultado!.EstaDisponivel)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine($"[AVISO] {perfume.Nome}: Produto esgotado no site.");
        Console.ResetColor();
    }
    else
    {
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

    Console.WriteLine(new string('-', 50));
}

static IScraper GetScraper(string url)
{
    var host = new Uri(url).Host;

    if (host.Contains("granado.com.br"))          return new GranadoScraper();
    if (host.Contains("nuancielo.com.br"))         return new NuancieloScraper();
    if (host.Contains("intheboxperfumes.com.br"))  return new InTheBoxScraper();
    if (host.Contains("natura.com.br"))            return new NaturaScraper();
    if (host.Contains("avatim.com.br"))            return new AvatimScraper();
    if (host.Contains("amazon.com.br"))            return new AmazonScraper();
    if (host.Contains("zara.com"))                 return new ZaraScraper();
    if (host.Contains("theracosmeticos.com.br"))   return new TheraScraper();
    if (host.Contains("boticario.com.br"))         return new BoticarioScraper();
    if (host.Contains("mahogany.com.br"))          return new MahoganyScraper();
    if (host.Contains("maisonviegas.com.br"))      return new MaisonViegasScraper();

    throw new NotSupportedException($"Domínio não suportado: {host}");
}
