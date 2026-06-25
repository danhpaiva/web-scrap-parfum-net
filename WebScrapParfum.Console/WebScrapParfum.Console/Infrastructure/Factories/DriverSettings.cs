namespace WebScrapParfum.Infrastructure.Factories;

public record DriverSettings(
    bool AddUserAgent = true,
    bool DisableBlinkAutomation = false,
    bool ExcludeEnableAutomation = false
);
