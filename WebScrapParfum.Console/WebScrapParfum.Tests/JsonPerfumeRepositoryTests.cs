using WebScrapParfum.Infrastructure.Repositories;

namespace WebScrapParfum.Tests;

public class JsonPerfumeRepositoryTests : IDisposable
{
    private readonly string _tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.json");

    [Fact]
    public void GetAll_ArquivoInexistente_DeveLancarFileNotFoundException()
    {
        var repository = new JsonPerfumeRepository(_tempFile);

        Assert.Throws<FileNotFoundException>(() => repository.GetAll());
    }

    [Fact]
    public void GetAll_JsonValido_DeveRetornarPerfumesDeserializados()
    {
        File.WriteAllText(_tempFile, """
        [
          { "Nome": "Bossa", "Url": "https://exemplo.com/bossa", "PrecoBase": 195.00 },
          { "Nome": "Wild", "Url": "https://exemplo.com/wild", "PrecoBase": 169.90 }
        ]
        """);

        var repository = new JsonPerfumeRepository(_tempFile);
        var perfumes = repository.GetAll();

        Assert.Equal(2, perfumes.Count);
        Assert.Equal("Bossa", perfumes[0].Nome);
        Assert.Equal(169.90m, perfumes[1].PrecoBase);
    }

    [Fact]
    public void GetAll_ConteudoJsonNull_DeveLancarInvalidOperationException()
    {
        File.WriteAllText(_tempFile, "null");

        var repository = new JsonPerfumeRepository(_tempFile);

        Assert.Throws<InvalidOperationException>(() => repository.GetAll());
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
            File.Delete(_tempFile);
    }
}
