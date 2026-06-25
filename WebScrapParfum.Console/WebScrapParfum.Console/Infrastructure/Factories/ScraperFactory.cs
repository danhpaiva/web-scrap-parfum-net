using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Infrastructure.Scrapers;

namespace WebScrapParfum.Infrastructure.Factories;

public class ScraperFactory : IScraperFactory
{
    public IScraper Create(string url)
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
        if (host.Contains("mercadolivre.com.br"))      return new MercadoLivreScraper();

        throw new NotSupportedException($"Domínio não suportado: {host}");
    }
}
