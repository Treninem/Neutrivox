using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

/// <summary>Formats a deployment plan for the UI and operator documentation without performing any transfer.</summary>
public sealed class DeploymentPlanFormatter
{
    public string Format(DeploymentPreview preview, bool english = false)
    {
        var sb = new StringBuilder();
        sb.AppendLine(english ? "Deployment preview" : "Предпросмотр передачи");
        sb.AppendLine(preview.Summary);
        sb.AppendLine();
        foreach (var target in preview.Plan.Targets.OrderBy(x => x.ProjectDeviceId))
        {
            sb.AppendLine($"• {target.DeviceName} [{target.DefinitionId}]");
            sb.AppendLine($"  {Translate(english, "Адрес", "Endpoint")}: {target.Endpoint ?? "—"}");
            sb.AppendLine($"  {Translate(english, "Идентификация", "Identification")}: {target.IdentificationState}");
        }

        if (preview.Plan.ValidationMessages.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine(english ? "Validation" : "Проверка");
            foreach (var message in preview.Plan.ValidationMessages) sb.AppendLine($"! {message}");
        }

        sb.AppendLine();
        sb.AppendLine(preview.RequiresUserConfirmation
            ? Translate(english, "Требуется явное подтверждение пользователя.", "Explicit user confirmation is required.")
            : Translate(english, "Передача не будет выполнена.", "No transfer will be performed."));
        return sb.ToString();
    }

    private static string Translate(bool english, string ru, string en) => english ? en : ru;
}
