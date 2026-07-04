using Microsoft.Extensions.Logging.Abstractions;
using WebScrapParfum.Infrastructure.Factories;

namespace WebScrapParfum.Tests;

public class ScraperFactoryTests
{
    private readonly ScraperFactory _factory = new(NullLoggerFactory.Instance);

    [Theory]
    [InlineData("https://www.dominio-nao-cadastrado.com.br/produto")]
    [InlineData("https://outraloja.com/produto/123")]
    public void Create_ComDominioNaoSuportado_DeveLancarNotSupportedException(string url)
    {
        var ex = Assert.Throws<NotSupportedException>(() => _factory.Create(url));

        Assert.Contains(new Uri(url).Host, ex.Message);
    }

    [Fact]
    public void Create_ComUrlInvalida_DeveLancarUriFormatException()
    {
        Assert.Throws<UriFormatException>(() => _factory.Create("não-é-uma-url"));
    }
}
