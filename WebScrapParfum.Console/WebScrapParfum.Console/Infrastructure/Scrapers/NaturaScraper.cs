using Microsoft.Extensions.Logging;
using OpenQA.Selenium;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Infrastructure.Scrapers;

public class NaturaScraper : ScraperBase
{
    public NaturaScraper(ILogger<NaturaScraper> logger)
        : base(new DriverSettings(DisableBlinkAutomation: true), TimeSpan.FromSeconds(20), logger) { }

    // A página da Natura NÃO expõe o preço principal por um id/testid estável:
    // #product-price e [data-testid='price-value'] só aparecem nos cards de
    // produtos relacionados (carrossel "você também pode gostar"). O preço real
    // fica na barra de compra, num <span> sem id cujo texto é um preço único e
    // limpo ("R$ 164,90"). Selecionamos esse elemento-folha por características
    // (preço limpo, visível, fora de card/carrossel e não riscado), tornando o
    // scraper imune a mudanças de classe/id. Preço riscado = valor "de" numa
    // promoção; queremos o valor a pagar.
    private const string ScriptPreco = @"
        const re = /^R\$\s*\d{1,3}(?:\.\d{3})*,\d{2}$/;
        const achados = [];
        for (const el of document.querySelectorAll('span,p,strong,b,div')) {
            const t = (el.textContent || '').trim();
            if (!re.test(t)) continue;
            if (el.querySelector('*')) continue;                       // apenas folha
            if (el.closest(""[data-testid^='product-card']"")) continue; // card de recomendação
            if (el.closest('.swiper-slide')) continue;                 // carrossel
            const r = el.getBoundingClientRect();
            if (r.width <= 0 || r.height <= 0) continue;               // visível
            const riscado = getComputedStyle(el).textDecorationLine.includes('line-through');
            achados.push({ t, riscado, top: r.top });
        }
        if (achados.length === 0) return null;
        const aPagar = achados.filter(x => !x.riscado);
        const lista = aPagar.length ? aPagar : achados;
        lista.sort((a, b) => a.top - b.top);
        return lista[0].t;";

    protected override ScrapedResult Execute(PerfumeConfig config)
    {
        _driver.Navigate().GoToUrl(config.Url);

        try
        {
            var texto = _wait.Until(_ => LerPrecoPrincipal());
            return new ScrapedResult(config, ParsePrice(texto!), true);
        }
        catch (WebDriverTimeoutException)
        {
            bool esgotado = _driver.PageSource.Contains("Produto indisponível") ||
                            _driver.PageSource.Contains("Avise-me");

            return new ScrapedResult(config, 0, !esgotado);
        }
    }

    // Retorna o texto do preço principal, ou null enquanto a página ainda não o
    // renderizou — o WebDriverWait interpreta null como "ainda não pronto".
    private string? LerPrecoPrincipal()
        => ((IJavaScriptExecutor)_driver).ExecuteScript(ScriptPreco) as string;
}
