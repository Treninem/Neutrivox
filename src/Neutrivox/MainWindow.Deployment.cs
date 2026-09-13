using Avalonia.Controls;
using Avalonia.Layout;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly DeploymentAdapterRegistry _deploymentAdapters = new();
    private readonly HashSet<Guid> _deploymentSelection = [];
    private readonly List<string> _deploymentLog = [];
    private CancellationTokenSource? _deploymentCancellation;
    private bool _deploymentServicesInitialized;

    private void EnsureDeploymentServices()
    {
        if (_deploymentServicesInitialized) return;
        if (!_connectionServicesInitialized) EnsureConnectionServices();
        _deploymentAdapters.Register(new OwenReplicationUtilityAdapter());
        _deploymentServicesInitialized = true;
    }

    private void ShowDeploymentPreview()
    {
        EnsureDeploymentServices();
        SetHeader(
            T("Передача в приборы", "Deployment to devices"),
            T("Передача выполняется только по выбранным приборам, после preflight, точного профиля, проверенного адаптера и явного подтверждения.",
              "Deployment runs only for selected devices after preflight, an exact profile, a verified adapter and explicit confirmation."));
        PageContent.Children.Clear();

        if (_project is null)
        {
            AddAction(T("Создать проект", "Create project"), T("Для передачи сначала нужен проект.", "A project is required before deployment."), (_, _) => NewProjectButton_OnClick(null, null!));
            return;
        }

        if (!CanUsePhysicalDeviceIntegration())
        {
            PageContent.Children.Add(new TextBlock
            {
                Text = T("Физическая передача недоступна в текущей редакции лицензии. Откройте Настройки для trial/активации.",
                         "Physical deployment is not available in the current license edition. Open Settings for trial/activation."),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });
            return;
        }

        var bound = _project.Devices.Where(x => x.PhysicalBinding is not null).ToList();
        if (bound.Count == 0)
        {
            PageContent.Children.Add(new TextBlock { Text = T("Сначала сопоставьте хотя бы один физический прибор в разделе Подключение.", "Bind at least one physical device in Connection first."), TextWrapping = Avalonia.Media.TextWrapping.Wrap });
            return;
        }

        if (_deploymentSelection.Count == 0)
            foreach (var device in bound) _deploymentSelection.Add(device.Id);
        _deploymentSelection.RemoveWhere(id => bound.All(x => x.Id != id));

        PageContent.Children.Add(new TextBlock { Text = T("Выберите цели и порядок", "Select deployment targets"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        foreach (var device in bound)
        {
            var check = new CheckBox
            {
                Content = $"{device.Name} — {device.PhysicalBinding!.Endpoint}",
                IsChecked = _deploymentSelection.Contains(device.Id)
            };
            check.IsCheckedChanged += (_, _) =>
            {
                if (check.IsChecked == true) _deploymentSelection.Add(device.Id);
                else _deploymentSelection.Remove(device.Id);
            };
            PageContent.Children.Add(check);
        }

        var selectedIds = bound.Where(x => _deploymentSelection.Contains(x.Id)).Select(x => x.Id).ToList();
        var workflow = new DeploymentWorkflowService(new DeploymentPreflightService(), _profiles, _deploymentAdapters);
        var preview = workflow.BuildPreview(_project, selectedIds);

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 8) });
        PageContent.Children.Add(new TextBlock
        {
            Text = preview.CanProceed
                ? T("План прошёл предварительную проверку.", "The plan passed the preliminary check.")
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
            PageContent.Children.Add(new TextBlock { Text = T("Блокирующие причины", "Blocking reasons"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 12, 0, 2) });
            foreach (var error in preview.Errors)
                PageContent.Children.Add(new TextBlock { Text = "✖ " + error, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        }

        if (preview.Warnings.Count > 0)
        {
            PageContent.Children.Add(new TextBlock { Text = T("Предупреждения", "Warnings"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 12, 0, 2) });
            foreach (var warning in preview.Warnings)
                PageContent.Children.Add(new TextBlock { Text = "⚠ " + warning, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        }

        if (preview.CanProceed)
            AddDeploymentConfirmation(selectedIds);

        if (_deploymentLog.Count > 0)
        {
            PageContent.Children.Add(new TextBlock { Text = T("Журнал передачи", "Deployment log"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 12, 0, 2) });
            foreach (var line in _deploymentLog.TakeLast(40))
                PageContent.Children.Add(new TextBlock { Text = line, TextWrapping = Avalonia.Media.TextWrapping.Wrap, FontFamily = Avalonia.Media.FontFamily.Default });
        }

        var refresh = new Button { Content = T("Пересчитать план", "Rebuild plan"), HorizontalAlignment = HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 10) };
        refresh.Click += (_, _) => ShowDeploymentPreview();
        PageContent.Children.Add(refresh);
    }

    private void AddDeploymentConfirmation(IReadOnlyList<Guid> selectedIds)
    {
        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock
        {
            Text = T("ВНИМАНИЕ: следующая операция может изменить конфигурацию физического прибора. Введите DEPLOY для подтверждения.",
                     "WARNING: the next operation may change physical device configuration. Type DEPLOY to confirm."),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        var confirmation = new TextBox { Watermark = "DEPLOY" };
        PageContent.Children.Add(confirmation);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var execute = new Button { Content = T("Выполнить последовательную передачу", "Execute sequential deployment"), IsEnabled = _deploymentCancellation is null };
        execute.Click += async (_, _) =>
        {
            if (!string.Equals(confirmation.Text?.Trim(), "DEPLOY", StringComparison.Ordinal))
            {
                _deploymentLog.Add(T("Подтверждение не совпало. Передача не запускалась.", "Confirmation did not match. Deployment was not started."));
                ShowDeploymentPreview();
                return;
            }
            await ExecuteDeploymentAsync(selectedIds);
        };
        actions.Children.Add(execute);
        if (_deploymentCancellation is not null)
        {
            var cancel = new Button { Content = T("Отменить передачу", "Cancel deployment") };
            cancel.Click += (_, _) => _deploymentCancellation?.Cancel();
            actions.Children.Add(cancel);
        }
        PageContent.Children.Add(actions);
    }

    private async Task ExecuteDeploymentAsync(IReadOnlyList<Guid> selectedIds)
    {
        if (_project is null || selectedIds.Count == 0 || _deploymentCancellation is not null) return;
        var planning = new DeploymentPlanningService();
        var prepared = planning.CreatePreview(_project, selectedIds);
        if (prepared.Plan.State != DeploymentState.ReadyForConfirmation)
        {
            _deploymentLog.Add(T("План не готов к подтверждению.", "The plan is not ready for confirmation."));
            ShowDeploymentPreview();
            return;
        }

        _deploymentCancellation = new CancellationTokenSource();
        _deploymentLog.Clear();
        ShowDeploymentPreview();
        try
        {
            var preflight = new DeploymentPreflightService();
            var safety = new DeploymentSafetyGateService(preflight, _deploymentAdapters, _profiles);
            var guard = new DeploymentPlanGuardService(new DeploymentPlanFingerprintService());
            var execution = new DeploymentExecutionService(_deploymentAdapters, guard, safety);
            var result = await execution.ExecuteAsync(
                _project,
                prepared.Plan,
                BuildDeploymentContext,
                Environment.UserName,
                (_, item) =>
                {
                    _deploymentLog.Add($"#{item.Order} {item.DeviceName}: {item.State} — {item.Message}");
                    foreach (var step in item.Steps)
                        _deploymentLog.Add($"   {step.Step}: {(step.Success ? "OK" : "FAIL")} — {step.Message}");
                    return Task.CompletedTask;
                },
                _deploymentCancellation.Token);
            _deploymentLog.Add(result.Summary);
        }
        catch (OperationCanceledException)
        {
            _deploymentLog.Add(T("Передача отменена пользователем.", "Deployment was cancelled by the user."));
        }
        finally
        {
            _deploymentCancellation.Dispose();
            _deploymentCancellation = null;
            ShowDeploymentPreview();
        }
    }

    private DeploymentContext? BuildDeploymentContext(Guid deviceId)
    {
        if (_project is null) return null;
        var device = _project.Devices.FirstOrDefault(x => x.Id == deviceId);
        var binding = device?.PhysicalBinding;
        if (device is null || binding is null) return null;
        var match = _profiles.Match(binding.Manufacturer ?? string.Empty, binding.Model ?? string.Empty)
            .FirstOrDefault(x => x.Confidence >= 0.9);
        if (match is null) return null;

        var parameters = new Dictionary<string, object?>();
        if (!string.IsNullOrWhiteSpace(_settings.OwenReplicationUtilityPath))
            parameters["utilityPath"] = _settings.OwenReplicationUtilityPath;
        return new DeploymentContext(_project, device, match.Profile, binding.Endpoint, true, parameters);
    }
}
