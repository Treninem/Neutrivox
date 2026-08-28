namespace Neutrivox.Services;

public enum AppLanguage { Russian, English }

/// <summary>Small centralized translation service for code-generated UI text.</summary>
public sealed class LocalizationService
{
    public AppLanguage Language { get; private set; } = AppLanguage.Russian;

    public void SetLanguage(AppLanguage language) => Language = language;

    public string Get(string ru, string en) => Language == AppLanguage.English ? en : ru;

    public string LanguageCode => Language == AppLanguage.English ? "EN" : "RU";
}
