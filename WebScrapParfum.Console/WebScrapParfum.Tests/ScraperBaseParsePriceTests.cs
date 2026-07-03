using System.Reflection;
using WebScrapParfum.Infrastructure.Scrapers;

namespace WebScrapParfum.Tests;

public class ScraperBaseParsePriceTests
{
    private static readonly MethodInfo ParsePriceMethod = typeof(ScraperBase)
        .GetMethod("ParsePrice", BindingFlags.NonPublic | BindingFlags.Static)
        ?? throw new InvalidOperationException("Método ParsePrice não encontrado via reflection.");

    private static decimal ParsePrice(string texto)
    {
        try
        {
            return (decimal)ParsePriceMethod.Invoke(null, [texto])!;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null)
        {
            throw ex.InnerException;
        }
    }

    [Theory]
    [InlineData("R$ 195,00", 195.00)]
    [InlineData("R$81,90", 81.90)]
    [InlineData("  R$ 129,00  \n", 129.00)]
    [InlineData("Preço promocional R$ 179,90", 179.90)]
    [InlineData("ou 6x R$ 13,65", 13.65)]
    public void ParsePrice_ComTextoValido_DeveExtrairValorCorreto(string texto, decimal esperado)
    {
        Assert.Equal(esperado, ParsePrice(texto));
    }

    [Fact]
    public void ParsePrice_SemPadraoDePreco_DeveLancarFormatException()
    {
        Assert.Throws<FormatException>(() => ParsePrice("Produto indisponível"));
    }

    [Fact]
    public void ParsePrice_ComSeparadorDeMilhar_DeveExtrairValorCompleto()
    {
        // Bug conhecido: a regex atual (\d+,\d{2}) ignora o "1." em "1.234,56"
        // e captura apenas "234,56". Esse teste documenta o comportamento esperado.
        Assert.Equal(1234.56m, ParsePrice("R$ 1.234,56"));
    }
}
