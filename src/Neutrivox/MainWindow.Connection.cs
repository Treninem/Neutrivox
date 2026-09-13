using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly SerialPortInventoryService _serialPorts = new();
    private readonly DeviceDiscoveryService _discovery = new();
    private readonly DeviceDiscoveryService _networkDiscovery = DiscoveryProviderFactory.CreateDefault();
    private readonly DeviceProfileRegistry _profiles = new();
    private DeviceBindingWorkflowService _bindingWorkflow = null!;
    private bool _connectionServicesInitialized;
    private IReadOnlyList<DiscoveredDevice> _discoveredDevices = [];
    private string? _selectedSerialPort;
    private string _networkScope = "192.168.1.0/24";
    private CancellationTokenSource? _scanCancellation;
    private DeviceBindingCandidate? _pendingBinding;
    private Guid? _pendingBindingProjectDeviceId;
    private string? _connectionMessage;

    private void ConnectionNavigationButton_OnClick(object? sender, RoutedEventArgs e) => ShowConnectionCenter();

    private void EnsureConnectionServices()
    {
        if (_connectionServicesInitialized) return;
        BuiltInDeviceProfiles.RegisterVerifiedProfiles(_profiles);
        VerifiedOwenProfiles.Register(_profiles);
        VerifiedOwenGatewayProfiles.Register(_profiles);
        _discovery.Register(new ModbusSerialDiscoveryProvider());
        _bindingWorkflow = new DeviceBindingWorkflowService(_profiles);
        _connectionServicesInitialized = true;
    }

    private void ShowConnectionCenter()
    {
        EnsureConnectionServices();
        RefreshLicenseState();
        SetHeader(
            T("Подключение и обнаружение", "Connection & discovery"),
            T("Поиск ограничен выбранным COM-портом или явно заданной IPv4-областью. Сопоставление физического прибора всегда подтверждается отдельно.",
              "Discovery is limited to a selected COM port or an explicitly supplied IPv4 scope. Physical-device binding always requires separate confirmation."));
        PageContent.Children.Clear();

        var physicalAllowed = CanUsePhysicalDeviceIntegration();
        if (!physicalAllowed)
        {
            PageContent.Children.Add(new TextBlock
            {
                Text = T("Физическое подключение требует Professional/Business/Owner. Проектирование и симуляция остаются доступны.",
                         "Physical integration requires Professional/Business/Owner. Design and simulation remain available."),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });
        }

        var ports = _serialPorts.Enumerate();
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = T("RS-485 / COM", "RS-485 / COM"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });

        if (ports.Count == 0)
            panel.Children.Add(new TextBlock { Text = T("COM-порты не найдены. Подключите USB/RS-485 преобразователь или прибор и обновите список.", "No COM ports found. Connect a USB/RS-485 converter or device and refresh the list."), TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        else
        {
            foreach (var port in ports)
            {
                var button = new Button { Content = $"{port.PortName} — {port.Description}", HorizontalContentAlignment = HorizontalAlignment.Left };
                button.Click += (_, _) => { _selectedSerialPort = port.PortName; ShowConnectionCenter(); };
                panel.Children.Add(button);
            }
        }

        panel.Children.Add(new TextBlock { Text = T($"Выбран: {_selectedSerialPort ?? "не выбран"}", $"Selected: {_selectedSerialPort ?? "none"}"), Opacity = 0.7 });
        var serialActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var scan = new Button { Content = T("Проверить Modbus RTU", "Scan Modbus RTU"), IsEnabled = _selectedSerialPort is not null && _scanCancellation is null && physicalAllowed };
        scan.Click += async (_, _) => await ScanSerialAsync();
        var refresh = new Button { Content = T("Обновить COM-порты", "Refresh serial ports") };
        refresh.Click += (_, _) => ShowConnectionCenter();
        serialActions.Children.Add(scan);
        serialActions.Children.Add(refresh);
        panel.Children.Add(serialActions);

        panel.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 6) });
        panel.Children.Add(new TextBlock { Text = T("Ethernet / Modbus TCP endpoint", "Ethernet / Modbus TCP endpoint"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        var networkBox = new TextBox { Text = _networkScope, Watermark = "192.168.1.0/24" };
        panel.Children.Add(networkBox);
        var networkActions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var networkScan = new Button { Content = T("Сканировать TCP/502", "Scan TCP/502"), IsEnabled = _scanCancellation is null && physicalAllowed };
        networkScan.Click += async (_, _) =>
        {
            _networkScope = networkBox.Text?.Trim() ?? string.Empty;
            await ScanNetworkAsync();
        };
        networkActions.Children.Add(networkScan);
        if (_scanCancellation is not null)
        {
            var cancel = new Button { Content = T("Отмена", "Cancel") };
            cancel.Click += (_, _) => _scanCancellation?.Cancel();
            networkActions.Children.Add(cancel);
        }
        panel.Children.Add(networkActions);

        if (_project is not null && _project.Devices.Any(x => x.PhysicalBinding is not null))
        {
            var deployment = new Button { Content = T("Подготовить план передачи", "Prepare deployment plan"), Margin = new Avalonia.Thickness(0, 4, 0, 0), IsEnabled = physicalAllowed };
            deployment.Click += (_, _) => ShowDeploymentPreview();
            panel.Children.Add(deployment);
        }

        panel.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 8) });
        panel.Children.Add(new TextBlock { Text = T("Результат обнаружения", "Discovery result"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (!string.IsNullOrWhiteSpace(_connectionMessage))
            panel.Children.Add(new TextBlock { Text = _connectionMessage, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

        if (_discoveredDevices.Count == 0)
            panel.Children.Add(new TextBlock { Text = _scanCancellation is null ? T("Устройств с подтверждённым ответом пока нет.", "No devices with a verified response yet.") : T("Выполняется поиск…", "Discovery is running…"), Opacity = 0.65 });
        else
            foreach (var device in _discoveredDevices)
                AddDiscoveredDeviceCard(panel, device);

        AddPendingBindingCard(panel);
        PageContent.Children.Add(panel);
    }

    private async Task ScanSerialAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedSerialPort) || _scanCancellation is not null) return;
        EnsureConnectionServices();
        _scanCancellation = new CancellationTokenSource();
        _connectionMessage = null;
        ShowConnectionCenter();
        try
        {
            var result = await new DeviceDiscoveryWorkflowService(_discovery).RunAsync(_selectedSerialPort, includeEthernet: false, includeSerial: true, _scanCancellation.Token);
            _discoveredDevices = result.Success ? result.Devices : [];
            if (!result.Success) _connectionMessage = string.Join(" ", result.Errors);
        }
        finally
        {
            _scanCancellation.Dispose();
            _scanCancellation = null;
            ShowConnectionCenter();
        }
    }

    private async Task ScanNetworkAsync()
    {
        if (_scanCancellation is not null) return;
        _scanCancellation = new CancellationTokenSource();
        _connectionMessage = null;
        ShowConnectionCenter();
        try
        {
            var result = await new DeviceDiscoveryWorkflowService(_networkDiscovery).RunAsync(_networkScope, includeEthernet: true, includeSerial: false, _scanCancellation.Token);
            _discoveredDevices = result.Success ? result.Devices : [];
            if (!result.Success) _connectionMessage = string.Join(" ", result.Errors);
        }
        finally
        {
            _scanCancellation.Dispose();
            _scanCancellation = null;
            ShowConnectionCenter();
        }
    }

    private void AddDiscoveredDeviceCard(StackPanel parent, DiscoveredDevice discovered)
    {
        var box = new Border { Padding = new Avalonia.Thickness(10), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
        var stack = new StackPanel { Spacing = 5 };
        stack.Children.Add(new TextBlock { Text = discovered.Endpoint, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        stack.Children.Add(new TextBlock { Text = $"{discovered.Protocol} • {discovered.IdentificationState}", Opacity = 0.7 });
        stack.Children.Add(new TextBlock { Text = discovered.Model is null
            ? T("Endpoint найден, но модель не заявляется без подтверждённой идентификации.", "Endpoint found, but no model is claimed without verified identification.")
            : $"{discovered.Manufacturer} {discovered.Model}", Opacity = 0.65, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        if (_project is not null && _project.Devices.Count > 0)
        {
            foreach (var projectDevice in _project.Devices)
            {
                var bind = new Button { Content = T($"Проверить сопоставление: {projectDevice.Name}", $"Check binding: {projectDevice.Name}") };
                bind.Click += (_, _) => PrepareBinding(projectDevice, discovered);
                stack.Children.Add(bind);
            }
        }
        box.Child = stack;
        parent.Children.Add(box);
    }

    private void PrepareBinding(ProjectDevice projectDevice, DiscoveredDevice discovered)
    {
        EnsureConnectionServices();
        _pendingBinding = _bindingWorkflow.BuildCandidates(projectDevice, [discovered]).FirstOrDefault();
        _pendingBindingProjectDeviceId = projectDevice.Id;
        _connectionMessage = _pendingBinding?.ProfileMatch is null
            ? T("Сопоставление заблокировано: устройство не удалось идентифицировать по документированному профилю.", "Binding blocked: the device could not be identified against a documented profile.")
            : null;
        ShowConnectionCenter();
    }

    private void AddPendingBindingCard(StackPanel parent)
    {
        if (_pendingBinding is null || _project is null || _pendingBindingProjectDeviceId is not Guid projectDeviceId) return;
        var projectDevice = _project.Devices.FirstOrDefault(x => x.Id == projectDeviceId);
        if (projectDevice is null) return;

        var box = new Border { Padding = new Avalonia.Thickness(12), Background = Avalonia.Media.Brush.Parse("#16213D"), CornerRadius = new Avalonia.CornerRadius(8) };
        var stack = new StackPanel { Spacing = 5 };
        stack.Children.Add(new TextBlock { Text = T("Подтверждение сопоставления", "Confirm device binding"), FontSize = 17, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        stack.Children.Add(new TextBlock { Text = $"{projectDevice.Name} ↔ {_pendingBinding.Device.Endpoint}" });
        if (_pendingBinding.ProfileMatch is not null)
            stack.Children.Add(new TextBlock { Text = $"{_pendingBinding.ProfileMatch.Profile.Manufacturer} {_pendingBinding.ProfileMatch.Profile.ModelFamily} • {_pendingBinding.ProfileMatch.Confidence:P0}", Opacity = 0.75 });
        if (_pendingBinding.Compatibility is not null)
            stack.Children.Add(new TextBlock { Text = _pendingBinding.Compatibility.Message, TextWrapping = Avalonia.Media.TextWrapping.Wrap, Opacity = 0.75 });

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var confirm = new Button
        {
            Content = T("Подтвердить сопоставление", "Confirm binding"),
            IsEnabled = _pendingBinding.ProfileMatch is not null && _pendingBinding.Compatibility is not { Compatible: false }
        };
        confirm.Click += (_, _) => ConfirmPendingBinding(projectDevice);
        var cancel = new Button { Content = T("Отмена", "Cancel") };
        cancel.Click += (_, _) => { _pendingBinding = null; _pendingBindingProjectDeviceId = null; ShowConnectionCenter(); };
        actions.Children.Add(confirm); actions.Children.Add(cancel); stack.Children.Add(actions);
        box.Child = stack;
        parent.Children.Add(box);
    }

    private void ConfirmPendingBinding(ProjectDevice projectDevice)
    {
        if (_pendingBinding is null) return;
        var result = _bindingWorkflow.Confirm(projectDevice, _pendingBinding);
        _connectionMessage = result.Success
            ? T("Физический прибор сопоставлен с объектом проекта.", "Physical device was bound to the project device.")
            : result.Message;
        if (result.Success) _ = CaptureRecoveryAsync();
        _pendingBinding = null;
        _pendingBindingProjectDeviceId = null;
        UpdateProjectPanel();
        ShowConnectionCenter();
    }
}
