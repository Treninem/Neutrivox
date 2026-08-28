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
    private readonly DeviceProfileRegistry _profiles = new();
    private DeviceBindingWorkflowService _bindingWorkflow = null!;
    private bool _connectionServicesInitialized;
    private IReadOnlyList<DiscoveredDevice> _discoveredDevices = [];
    private string? _selectedSerialPort;

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
        SetHeader(
            T("Подключение и обнаружение", "Connection & discovery"),
            T("Выберите реальный интерфейс. Программа сначала обнаруживает устройства, затем показывает, что удалось подтвердить.",
              "Choose a physical interface. The application discovers devices first and then shows what could actually be verified."));
        PageContent.Children.Clear();

        var ports = _serialPorts.Enumerate();
        var panel = new StackPanel { Spacing = 10 };
        panel.Children.Add(new TextBlock { Text = T("Последовательные порты", "Serial ports"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });

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

        var scan = new Button { Content = T("Проверить Modbus RTU", "Scan Modbus RTU"), IsEnabled = _selectedSerialPort is not null };
        scan.Click += async (_, _) => await ScanSerialAsync();
        panel.Children.Add(scan);

        var refresh = new Button { Content = T("Обновить COM-порты", "Refresh serial ports") };
        refresh.Click += (_, _) => ShowConnectionCenter();
        panel.Children.Add(refresh);

        if (_project is not null && _project.Devices.Any(x => x.PhysicalBinding is not null))
        {
            var deployment = new Button { Content = T("Подготовить план передачи", "Prepare deployment plan"), Margin = new Avalonia.Thickness(0, 4, 0, 0) };
            deployment.Click += (_, _) => ShowDeploymentPreview();
            panel.Children.Add(deployment);
        }

        panel.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 8) });
        panel.Children.Add(new TextBlock { Text = T("Результат обнаружения", "Discovery result"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });

        if (_discoveredDevices.Count == 0)
            panel.Children.Add(new TextBlock { Text = T("Устройств с подтверждённым ответом пока нет.", "No devices with a verified response yet."), Opacity = 0.65 });
        else
            foreach (var device in _discoveredDevices)
                AddDiscoveredDeviceCard(panel, device);

        PageContent.Children.Add(panel);
    }

    private async Task ScanSerialAsync()
    {
        if (string.IsNullOrWhiteSpace(_selectedSerialPort)) return;
        EnsureConnectionServices();
        var result = await new DeviceDiscoveryWorkflowService(_discovery).RunAsync(_selectedSerialPort, includeEthernet: false, includeSerial: true);
        _discoveredDevices = result.Success ? result.Devices : [];
        ShowConnectionCenter();
    }

    private void AddDiscoveredDeviceCard(StackPanel parent, DiscoveredDevice discovered)
    {
        var box = new Border { Padding = new Avalonia.Thickness(10), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
        var stack = new StackPanel { Spacing = 5 };
        stack.Children.Add(new TextBlock { Text = discovered.Endpoint, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        stack.Children.Add(new TextBlock { Text = $"{discovered.Protocol} • {discovered.IdentificationState}", Opacity = 0.7 });
        stack.Children.Add(new TextBlock { Text = T("Модель не заявляется без подтверждения идентификации.", "Model is not claimed without verified identification."), Opacity = 0.65, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        if (_project is not null && _project.Devices.Count > 0)
        {
            foreach (var projectDevice in _project.Devices)
            {
                var bind = new Button { Content = T($"Предложить сопоставление: {projectDevice.Name}", $"Suggest binding: {projectDevice.Name}") };
                bind.Click += (_, _) => BindDiscovered(projectDevice, discovered);
                stack.Children.Add(bind);
            }
        }
        box.Child = stack;
        parent.Children.Add(box);
    }

    private void BindDiscovered(ProjectDevice projectDevice, DiscoveredDevice discovered)
    {
        EnsureConnectionServices();
        var candidates = _bindingWorkflow.BuildCandidates(projectDevice, [discovered]);
        var candidate = candidates.FirstOrDefault();
        if (candidate is null) return;
        _bindingWorkflow.Confirm(projectDevice, candidate);
        ShowConnectionCenter();
    }
}
