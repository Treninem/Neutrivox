using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly DeviceDiscoveryCoordinator _discoveryCoordinator;
    private readonly DeviceBindingWorkflowService _bindingWorkflow;
    private string _discoveryScope = "192.168.1.0/24";
    private IReadOnlyList<DeviceDiscoveryCandidate> _discoveryCandidates = [];
    private bool _discoveryBusy;

    private void InitializeDiscoveryServices()
    {
        var discovery = DiscoveryProviderFactory.CreateDefault();
        var profiles = VerifiedOwenCatalogBootstrap.CreateRegistry();
        var workflow = new DeviceDiscoveryWorkflowService(discovery);
        _discoveryCoordinator = new DeviceDiscoveryCoordinator(workflow, profiles);
        _bindingWorkflow = new DeviceBindingWorkflowService(profiles);
    }

    private async void ShowDiscovery()
    {
        SetHeader(
            T("Обнаружение оборудования", "Device discovery"),
            T("Найдите доступные устройства в явно заданной области, проверьте профиль и только затем сопоставьте прибор с проектом.",
              "Find devices in an explicitly requested scope, verify the profile, and only then bind a device to the project."));
        PageContent.Children.Clear();

        if (_discoveryCoordinator is null) InitializeDiscoveryServices();

        var scopeLabel = new TextBlock { Text = T("Область поиска", "Discovery scope"), FontWeight = Avalonia.Media.FontWeight.SemiBold };
        var scope = new TextBox { Text = _discoveryScope, Watermark = T("Например: 192.168.1.0/24 или 192.168.1.50", "Example: 192.168.1.0/24 or 192.168.1.50") };
        var scan = new Button { Content = _discoveryBusy ? T("Поиск...", "Scanning...") : T("Сканировать", "Scan") };
        var cancel = new Button { Content = T("Очистить", "Clear") };
        scan.IsEnabled = !_discoveryBusy;
        scan.Click += async (_, _) =>
        {
            _discoveryScope = scope.Text?.Trim() ?? string.Empty;
            _discoveryBusy = true;
            ShowDiscovery();
            try
            {
                var result = await _discoveryCoordinator.ScanAsync(_discoveryScope);
                _discoveryCandidates = result.Candidates;
            }
            finally
            {
                _discoveryBusy = false;
                ShowDiscovery();
            }
        };
        cancel.Click += (_, _) => { _discoveryCandidates = []; ShowDiscovery(); };

        var controls = new Grid { ColumnDefinitions = new ColumnDefinitions("Auto,*,Auto,Auto"), ColumnSpacing = 8 };
        Grid.SetColumn(scopeLabel, 0); controls.Children.Add(scopeLabel);
        Grid.SetColumn(scope, 1); controls.Children.Add(scope);
        Grid.SetColumn(scan, 2); controls.Children.Add(scan);
        Grid.SetColumn(cancel, 3); controls.Children.Add(cancel);
        PageContent.Children.Add(controls);

        if (_discoveryCandidates.Count == 0)
        {
            PageContent.Children.Add(new TextBlock
            {
                Text = _discoveryBusy
                    ? T("Идёт поиск доступных endpoint'ов...", "Searching reachable endpoints...")
                    : T("Устройства ещё не найдены. Поиск выполняется только в указанной области.", "No devices found yet. Discovery is limited to the specified scope."),
                Opacity = 0.7,
                Margin = new Avalonia.Thickness(0, 12, 0, 0)
            });
            return;
        }

        PageContent.Children.Add(new TextBlock { Text = T($"Найдено кандидатов: {_discoveryCandidates.Count}", $"Candidates found: {_discoveryCandidates.Count}"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 12, 0, 4) });

        foreach (var candidate in _discoveryCandidates)
        {
            var card = new Border { Padding = new Avalonia.Thickness(12), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
            var panel = new StackPanel { Spacing = 5 };
            panel.Children.Add(new TextBlock { Text = candidate.Device.Endpoint, FontSize = 17, FontWeight = Avalonia.Media.FontWeight.SemiBold });
            panel.Children.Add(new TextBlock { Text = $"{candidate.Device.Protocol} • {candidate.Device.Manufacturer ?? T("Производитель не определён", "Manufacturer unknown")} • {candidate.Device.Model ?? T("Модель не определена", "Model unknown")}", Opacity = 0.7 });
            panel.Children.Add(new TextBlock { Text = T($"Состояние: {candidate.Device.IdentificationState}", $"Identification: {candidate.Device.IdentificationState}"), Opacity = 0.7 });
            panel.Children.Add(new TextBlock { Text = candidate.Message, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

            foreach (var match in candidate.ProfileMatches.Take(3))
            {
                panel.Children.Add(new TextBlock { Text = T($"Профиль: {match.Profile.Manufacturer} {match.Profile.ModelFamily} • совпадение {match.Confidence:P0}", $"Profile: {match.Profile.Manufacturer} {match.Profile.ModelFamily} • match {match.Confidence:P0}"), Opacity = 0.75 });
            }

            var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Avalonia.Thickness(0, 5, 0, 0) };
            if (_project is not null && candidate.CanBind)
            {
                var bind = new Button { Content = T("Сопоставить с выбранным устройством проекта", "Bind to selected project device") };
                bind.Click += (_, _) => BindCandidate(candidate);
                actions.Children.Add(bind);
            }
            panel.Children.Add(actions);
            card.Child = panel;
            PageContent.Children.Add(card);
        }
    }

    private void BindCandidate(DeviceDiscoveryCandidate candidate)
    {
        if (_project is null || _workspace.Selection.DeviceId is not Guid selectedId) return;
        var projectDevice = _project.Devices.FirstOrDefault(x => x.Id == selectedId);
        if (projectDevice is null) return;
        var candidateBinding = _bindingWorkflow.BuildCandidates(projectDevice, [candidate.Device]).FirstOrDefault();
        if (candidateBinding is null) return;
        var result = _bindingWorkflow.Confirm(projectDevice, candidateBinding);
        if (result.Success) ShowWorkspace(); else ShowDiscovery();
    }
}
