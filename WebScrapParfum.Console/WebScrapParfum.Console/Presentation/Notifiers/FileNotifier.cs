using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Presentation.Notifiers;

public class FileNotifier : INotifier, IDisposable
{
    private readonly StreamWriter _writer;
    private bool _disposed;

    public FileNotifier(string filePath)
    {
        _writer = new StreamWriter(filePath, append: false, encoding: System.Text.Encoding.UTF8);
    }

    public void NotifyStarting(int total)
    {
        _writer.WriteLine($"Monitoramento de perfumes — {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
        _writer.WriteLine($"Total monitorado: {total} perfume(s)");
        _writer.WriteLine(new string('=', 50));
        _writer.WriteLine();
    }

    public void NotifyResult(ScrapedResult result)
    {
        _writer.WriteLine($"Perfume : {result.Info.Nome}");
        _writer.WriteLine($"URL     : {result.Info.Url}");

        if (!result.EstaDisponivel)
        {
            _writer.WriteLine("Status  : Esgotado");
        }
        else if (result.TemDesconto)
        {
            _writer.WriteLine($"Preço   : {result.PrecoAtual:C}");
            _writer.WriteLine($"Base    : {result.Info.PrecoBase:C}");
            _writer.WriteLine($"Status  : PROMOÇÃO — desconto de {result.ValorDesconto:C}");
        }
        else
        {
            _writer.WriteLine($"Preço   : {result.PrecoAtual:C}");
            _writer.WriteLine($"Base    : {result.Info.PrecoBase:C}");
            _writer.WriteLine("Status  : Sem desconto");
        }

        _writer.WriteLine(new string('-', 50));
        _writer.WriteLine();
    }

    public void NotifyError(PerfumeConfig config, string message)
    {
        _writer.WriteLine($"Perfume : {config.Nome}");
        _writer.WriteLine($"URL     : {config.Url}");
        _writer.WriteLine($"Status  : ERRO — {message}");
        _writer.WriteLine(new string('-', 50));
        _writer.WriteLine();
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _writer.Flush();
        _writer.Dispose();
        GC.SuppressFinalize(this);
    }
}
