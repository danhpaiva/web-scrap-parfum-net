using Microsoft.Extensions.Logging;
using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Infrastructure.Scrapers;

namespace WebScrapParfum.Infrastructure.Factories;

public class ScraperFactory : IScraperFactory
{
    private readonly ILoggerFactory _loggerFactory;

    public ScraperFactory(ILoggerFactory loggerFactory)
    {
        _loggerFactory = loggerFactory;
    }

    public IScraper Create(string url)
    {
        var host = new Uri(url).Host;

        if (host.Contains("granado.com.br"))          return new GranadoScraper(_loggerFactory.CreateLogger<GranadoScraper>());
        if (host.Contains("nuancielo.com.br"))         return new NuancieloScraper(_loggerFactory.CreateLogger<NuancieloScraper>());
        if (host.Contains("intheboxperfumes.com.br"))  return new InTheBoxScraper(_loggerFactory.CreateLogger<InTheBoxScraper>());
        if (host.Contains("natura.com.br"))            return new NaturaScraper(_loggerFactory.CreateLogger<NaturaScraper>());
        if (host.Contains("avatim.com.br"))            return new AvatimScraper(_loggerFactory.CreateLogger<AvatimScraper>());
        if (host.Contains("amazon.com.br"))            return new AmazonScraper(_loggerFactory.CreateLogger<AmazonScraper>());
        if (host.Contains("zara.com"))                 return new ZaraScraper(_loggerFactory.CreateLogger<ZaraScraper>());
        if (host.Contains("theracosmeticos.com.br"))   return new TheraScraper(_loggerFactory.CreateLogger<TheraScraper>());
        if (host.Contains("boticario.com.br"))         return new BoticarioScraper(_loggerFactory.CreateLogger<BoticarioScraper>());
        if (host.Contains("mahogany.com.br"))          return new MahoganyScraper(_loggerFactory.CreateLogger<MahoganyScraper>());
        if (host.Contains("maisonviegas.com.br"))      return new MaisonViegasScraper(_loggerFactory.CreateLogger<MaisonViegasScraper>());
        if (host.Contains("mercadolivre.com.br"))      return new MercadoLivreScraper(_loggerFactory.CreateLogger<MercadoLivreScraper>());
        if (host.Contains("wepink.com.br"))            return new WepinkScraper(_loggerFactory.CreateLogger<WepinkScraper>());

        throw new NotSupportedException($"Domínio não suportado: {host}");
    }
}
