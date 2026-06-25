using WebScrapParfum.Application.Interfaces;
using WebScrapParfum.Domain.Entities;
using WebScrapParfum.Domain.ValueObjects;

namespace WebScrapParfum.Presentation.Notifiers;

public class ConsoleNotifier : INotifier
{
    public void NotifyStarting(int total)
    {
        Console.WriteLine($"[LOG] Iniciando monitoramento de {total} perfume(s) em paralelo...");
        Console.WriteLine(new string('-', 50));
    }

    public void NotifyResult(ScrapedResult result)
    {
        Console.WriteLine($"[LOG] Verificado: {result.Info.Nome}");

        if (!result.EstaDisponivel)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"[AVISO] {result.Info.Nome}: Produto esgotado no site.");
            Console.ResetColor();
        }
        else if (result.TemDesconto)
        {
            Console.WriteLine($"[LOG] Preço encontrado: {result.PrecoAtual:C} (Base: {result.Info.PrecoBase:C})");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"!!! PROMOÇÃO DETECTADA !!!");
            Console.WriteLine($"Perfume: {result.Info.Nome}");
            Console.WriteLine($"Desconto de: {result.ValorDesconto:C}");
            Console.ResetColor();
        }
        else
        {
            Console.WriteLine($"[LOG] Preço encontrado: {result.PrecoAtual:C} (Base: {result.Info.PrecoBase:C})");
            Console.WriteLine($"[INFO] Sem desconto relevante para {result.Info.Nome}.");
        }

        Console.WriteLine(new string('-', 50));
    }

    public void NotifyError(PerfumeConfig config, string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[AVISO] Não foi possível processar {config.Nome}: {message}");
        Console.ResetColor();
        Console.WriteLine(new string('-', 50));
    }
}
