using System.Text.Json;
using WebScrapParfum.Domain.Entities;

namespace WebScrapParfum.Infrastructure.Repositories;

// Carregador da lista de seed. A lista viva (perfumes + histórico) é mantida
// no SQLite via SqlitePerfumeRepository; este tipo só lê o perfumes.json usado
// para semear o banco na primeira execução.
public class JsonPerfumeRepository
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
