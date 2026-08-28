using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly DeploymentAdapterRegistry _deploymentAdapters = new();
    private bool _deploymentServicesInitialized;

    private void EnsureDeploymentServices()
    {
        if (_deploymentServicesInitialized) return;
        if (!_connectionServicesInitialized) EnsureConnectionServices();
        _deploymentServicesInitialized = true;
    }

    private void ShowDeploymentPreview()
    {
        EnsureDeploymentServices();
        SetHeader(
            T("Передача в приборы", "Deployment to devices"),
            T("Сначала формируется безопасный план по каждому прибору. Физическая запись не выполняется на этом экране.",
              "A safe plan is prepared for each device first. No physical write is performed on this screen."));
        PageContent.Children.Clear();

        if (_project is null)
        {
            AddAction(T("Создать проект", "Create project"), T("Для передачи сначала нужен проект.", "A project is required before deployment."), (_, _) => NewProjectButton_OnClick(null, null));
            return;
        }

        var ids = _project.Devices.Select(x => x.Id).ToList();
        var workflow = new DeploymentWorkflowService(new DeploymentPreflightService(), _profiles, _deploymentAdapters);
        var preview = workflow.BuildPreview(_project, ids);

        PageContent.Children.Add(new TextBlock
        {
            Text = preview.CanProceed
                ? T("План предварительно готов.", "The preliminary plan is ready.")
                : T("Передача заблокирована до устранения указанных проблем.", "Deployment is blocked until the listed problems are resolved."),
            FontSize = 19,
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });

        foreach (var item in preview.Items)
        {
            var card = new Border
            {
                Padding = new Avalonia.Thickness(12),
                Margin = new Avalonia.Thickness(0, 4),
                Background = Avalonia.Media.Brush.Parse("#0D152B"),
                CornerRadius = new Avalonia.CornerRadius(8)
            };
            var stack = new StackPanel { Spacing = 5 };
            stack.Children.Add(new TextBlock { Text = $"{item.Order}. {item.DeviceName}", FontSize = 17, FontWeight = Avalonia.Media.FontWeight.SemiBold });
            stack.Children.Add(new TextBlock { Text = T($"Адрес: {item.Endpoint}", $"Endpoint: {item.Endpoint}"), Opacity = 0.75 });
            stack.Children.Add(new TextBlock { Text = $"Profile: {item.ProfileId}", Opacity = 0.65 });
            stack.Children.Add(new TextBlock { Text = _english ? item.StatusEn : item.StatusRu, Opacity = 0.8 });
            card.Child = stack;
            PageContent.Children.Add(card);
        }

        if (preview.Errors.Count > 0)
        {
            PageContent.Children.Add(new TextBlock { Text = T("Ошибки", "Errors"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 12, 0, 2) });
            foreach (var error in preview.Errors)
                PageContent.Children.Add(new TextBlock { Text = "✖ " + error, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        }

        if (preview.Warnings.Count > 0)
        {
            PageContent.Children.Add(new TextBlock { Text = T("Предупреждения", "Warnings"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 12, 0, 2) });
            foreach (var warning in preview.Warnings)
                PageContent.Children.Add(new TextBlock { Text = "⚠ " + warning, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        }

        var refresh = new Button { Content = T("Пересчитать план", "Rebuild plan"), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 10) };
        refresh.Click += (_, _) => ShowDeploymentPreview();
        PageContent.Children.Add(refresh);
    }
}
