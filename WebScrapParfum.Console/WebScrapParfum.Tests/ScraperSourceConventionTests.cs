using System.Text.RegularExpressions;
using Xunit.Abstractions;

namespace WebScrapParfum.Tests;

/// <summary>
/// Testes de convenção que analisam o código-fonte dos scrapers (sem abrir navegador).
/// Não substituem testes de integração contra os sites reais — servem para pegar
/// regressões estruturais como as encontradas no GranadoScraper.
/// </summary>
public class ScraperSourceConventionTests
{
    private readonly ITestOutputHelper _output;

    public ScraperSourceConventionTests(ITestOutputHelper output)
    {
        _output = output;
    }

    // Scrapers sem DisableBlinkAutomation/ExcludeEnableAutomation identificados em auditoria manual.
    // Pode ser proposital (site sem Cloudflare/detecção de bot) — aguardando revisão caso a caso.
    private static readonly HashSet<string> ScrapersAguardandoRevisaoAntiBot = new()
    {
        "AvatimScraper.cs",
        "AmazonScraper.cs",
        "TheraScraper.cs",
        "MaisonViegasScraper.cs",
        "NaturaScraper.cs",
        "InTheBoxScraper.cs",
    };

    private static string GetScrapersDirectory()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "WebScrapParfum.Console.slnx")))
            dir = dir.Parent;

        if (dir is null)
            throw new InvalidOperationException("Não foi possível localizar a raiz da solução a partir de " + AppContext.BaseDirectory);

        return Path.Combine(dir.FullName, "WebScrapParfum.Console", "Infrastructure", "Scrapers");
    }

    private static IEnumerable<string> GetScraperFiles() =>
        Directory.GetFiles(GetScrapersDirectory(), "*.cs")
            .Where(f => Path.GetFileName(f) != "ScraperBase.cs");

    [Fact]
    public void Scrapers_NaoDevemUsarXPathBaseadoApenasEmTextoLivre()
    {
        var xpathRegex = new Regex(@"By\.XPath\(\s*""([^""]+)""\s*\)");
        var ofensores = new List<string>();

        foreach (var arquivo in GetScraperFiles())
        {
            var conteudo = File.ReadAllText(arquivo);

            foreach (Match match in xpathRegex.Matches(conteudo))
            {
                var xpath = match.Groups[1].Value;
                bool buscaPorTextoLivre = xpath.Contains("contains(.,") || xpath.Contains("contains(text(),");
                bool restringePorAtributo = xpath.Contains('@');

                if (buscaPorTextoLivre && !restringePorAtributo)
                    ofensores.Add($"{Path.GetFileName(arquivo)}: {xpath}");
            }
        }

        Assert.True(ofensores.Count == 0,
            "Seletor XPath baseado apenas em texto livre (contains(., ...) sem restrição por @class/@id) " +
            "casa com qualquer elemento da página que contenha aquele texto (banners, parcelamento, etc). " +
            "Essa foi a causa raiz do bug original do GranadoScraper. Ofensores encontrados: " +
            string.Join(" | ", ofensores));
    }

    [Fact]
    public void Scrapers_DevemDeclararFlagsAntiDeteccaoOuEstarNaListaDeRevisao()
    {
        var driverSettingsRegex = new Regex(@"new DriverSettings\(([^)]*)\)");
        var regressoes = new List<string>();
        var revisaoPendente = new List<string>();

        foreach (var arquivo in GetScraperFiles())
        {
            var nomeArquivo = Path.GetFileName(arquivo);
            var match = driverSettingsRegex.Match(File.ReadAllText(arquivo));

            if (!match.Success)
                continue;

            var args = match.Groups[1].Value;
            bool temAmbasFlags = args.Contains("DisableBlinkAutomation: true") && args.Contains("ExcludeEnableAutomation: true");

            if (temAmbasFlags)
                continue;

            if (ScrapersAguardandoRevisaoAntiBot.Contains(nomeArquivo))
                revisaoPendente.Add(nomeArquivo);
            else
                regressoes.Add(nomeArquivo);
        }

        if (revisaoPendente.Count > 0)
        {
            _output.WriteLine(
                "Scrapers sem DisableBlinkAutomation/ExcludeEnableAutomation, aguardando revisão manual " +
                "(pode ser intencional se o site não usa Cloudflare/detecção de bot): " +
                string.Join(", ", revisaoPendente));
        }

        Assert.True(regressoes.Count == 0,
            "Scraper(s) novo(s) sem flags anti-detecção e fora da lista de revisão conhecida. " +
            "Adicione DisableBlinkAutomation/ExcludeEnableAutomation ou inclua em ScrapersAguardandoRevisaoAntiBot " +
            "se for proposital: " + string.Join(", ", regressoes));
    }
}
