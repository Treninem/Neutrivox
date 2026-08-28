using System.Text;
using Neutrivox.Models;

namespace Neutrivox.Services;

public sealed class DeploymentReportService
{
    public string Build(DeploymentExecutionResult result, AppLanguage language = AppLanguage.Russian)
    {
        var en = language == AppLanguage.English;
        var sb = new StringBuilder();
        sb.AppendLine(en ? "Neutrivox deployment report" : "Отчёт передачи Neutrivox");
        sb.AppendLine(en
            ? $"Status: {(result.Success ? "SUCCESS" : result.UserCancelled ? "CANCELLED" : "NOT COMPLETE")}"
            : $"Статус: {(result.Success ? "УСПЕШНО" : result.UserCancelled ? "ОТМЕНЕНО" : "НЕ ЗАВЕРШЕНО")}");
        sb.AppendLine(en ? $"Generated UTC: {DateTime.UtcNow:O}" : $"Создано UTC: {DateTime.UtcNow:O}");
        sb.AppendLine();

        foreach (var item in result.Items.OrderBy(x => x.Order))
        {
            sb.AppendLine($"{item.Order}. {item.DeviceName} [{State(item.State, en)}]");
            sb.AppendLine($"   {item.Message}");
            foreach (var step in item.Steps)
                sb.AppendLine($"   - {step.Step}: {(step.Success ? (en ? "OK" : "УСПЕШНО") : (en ? "FAILED" : "ОШИБКА"))} — {step.Message} ({step.Duration.TotalMilliseconds:0} ms)");
        }

        sb.AppendLine();
        sb.AppendLine(en ? result.Summary : TranslateSummary(result.Summary));
        return sb.ToString();
    }

    private static string State(DeploymentState state, bool en) => en ? state switch
    {
        DeploymentState.Draft => "Draft", DeploymentState.Validated => "Validated", DeploymentState.ReadyForConfirmation => "Ready for confirmation",
        DeploymentState.Confirmed => "Confirmed", DeploymentState.Completed => "Completed", DeploymentState.Failed => "Failed", DeploymentState.Cancelled => "Cancelled", _ => state.ToString()
    } : state switch
    {
        DeploymentState.Draft => "Черновик", DeploymentState.Validated => "Проверено", DeploymentState.ReadyForConfirmation => "Ожидает подтверждения",
        DeploymentState.Confirmed => "Подтверждено", DeploymentState.Completed => "Выполнено", DeploymentState.Failed => "Ошибка", DeploymentState.Cancelled => "Отменено", _ => state.ToString()
    };

    private static string TranslateSummary(string summary) => summary switch
    {
        "All deployment targets completed successfully." => "Все устройства обработаны успешно.",
        "Deployment completed with failures or skipped targets." => "Передача завершена с ошибками или пропущенными устройствами.",
        "Deployment stopped because explicit user confirmation was not provided." => "Передача остановлена, потому что не было явного подтверждения пользователя.",
        _ => summary
    };
}
