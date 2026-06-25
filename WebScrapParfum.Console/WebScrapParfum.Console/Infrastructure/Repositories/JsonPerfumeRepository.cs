using System.Text.Json;
using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Domain.Entities;

namespace WebScrapParfum.Infrastructure.Repositories;

public class JsonPerfumeRepository : IPerfumeRepository
{
    private readonly string _jsonPath;

    public JsonPerfumeRepository(string jsonPath)
    {
        _jsonPath = jsonPath;
    }

    public IReadOnlyList<PerfumeConfig> GetAll()
    {
        if (!File.Exists(_jsonPath))
            throw new FileNotFoundException($"Arquivo de configuração não encontrado: {_jsonPath}");

        var json = File.ReadAllText(_jsonPath);

        return JsonSerializer.Deserialize<List<PerfumeConfig>>(json)
               ?? throw new InvalidOperationException("Arquivo de configuração inválido ou vazio.");
    }
}
