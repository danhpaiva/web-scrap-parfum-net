using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Tests;

public class ScrapedResultTests
{
    private static PerfumeConfig NovoPerfume(decimal precoBase) =>
        new("Perfume Teste", "https://exemplo.com/produto", precoBase);

    [Fact]
    public void TemDesconto_QuandoPrecoAtualMenorQueBaseEDisponivel_DeveSerTrue()
    {
        var resultado = new ScrapedResult(NovoPerfume(200.00m), 150.00m, EstaDisponivel: true);

        Assert.True(resultado.TemDesconto);
        Assert.Equal(50.00m, resultado.ValorDesconto);
    }

    [Fact]
    public void TemDesconto_QuandoPrecoAtualIgualOuMaiorQueBase_DeveSerFalse()
    {
        var resultado = new ScrapedResult(NovoPerfume(150.00m), 150.00m, EstaDisponivel: true);

        Assert.False(resultado.TemDesconto);
    }

    [Fact]
    public void TemDesconto_QuandoIndisponivel_DeveSerFalseMesmoComPrecoMenor()
    {
        var resultado = new ScrapedResult(NovoPerfume(200.00m), 100.00m, EstaDisponivel: false);

        Assert.False(resultado.TemDesconto);
    }

    [Fact]
    public void ValorDesconto_DeveSerDiferencaEntreBaseEAtual_MesmoSemDesconto()
    {
        var resultado = new ScrapedResult(NovoPerfume(100.00m), 120.00m);

        Assert.Equal(-20.00m, resultado.ValorDesconto);
    }
}
