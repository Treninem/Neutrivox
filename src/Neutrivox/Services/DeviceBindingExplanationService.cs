using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed record BindingExplanation(string Status, string TitleRu, string TitleEn, string DetailsRu, string DetailsEn);

/// <summary>Turns discovery/profile matching data into user-facing, non-ambiguous explanations.</summary>
public sealed class DeviceBindingExplanationService
{
    public BindingExplanation Explain(DeviceBindingCandidate candidate)
    {
        if (candidate.ProfileMatch is null)
            return new("Unknown", "Модель не подтверждена", "Model not confirmed",
                "Ответ устройства получен, но документированного профиля с достаточной идентификацией не найдено. Сопоставление не рекомендуется.",
                "A device response was received, but no documented profile with sufficient identification was found. Binding is not recommended.");

        if (candidate.Compatibility is { Compatible: false })
            return new("Incompatible", "Несовместимо", "Incompatible",
                "Найденная модель не соответствует конфигурации устройства проекта.",
                "The discovered model does not match the project device configuration.");

        var confidence = candidate.ProfileMatch.Confidence >= 0.9 ? "Высокая" : candidate.ProfileMatch.Confidence >= 0.6 ? "Средняя" : "Низкая";
        var confidenceEn = candidate.ProfileMatch.Confidence >= 0.9 ? "High" : candidate.ProfileMatch.Confidence >= 0.6 ? "Medium" : "Low";
        return new("Candidate", $"Кандидат: {candidate.ProfileMatch.Profile.ModelFamily}", $"Candidate: {candidate.ProfileMatch.Profile.ModelFamily}",
            $"Совпадение профиля: {confidence}. Причина: {candidate.ProfileMatch.Reason}",
            $"Profile match confidence: {confidenceEn}. Reason: {candidate.ProfileMatch.Reason}");
    }
}
