using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow : Window
{
    private readonly DeviceCatalogService _catalog = new();
    private readonly ProjectEquipmentService _equipment = new();
    private readonly ProjectWorkspaceController _workspace = new(new WorkspaceService());
    private readonly ProjectConnectionService _connections = new();
    private AutomationProject? _project;
    private bool _english;

    public MainWindow()
    {
        InitializeComponent();
        BuiltInDeviceCatalog.RegisterDefaults(_catalog);
        ShowWelcome();
    }

    private void NewProjectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _project = new AutomationProject { Name = T("Новый проект", "New project") };
        _workspace.LoadProject(_project);
        UpdateProjectPanel();
        ShowDevices();
    }

    private void NavigationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string page }) ShowPage(page);
    }

    private void ShowPage(string page)
    {
        switch (page)
        {
            case "Projects": ShowWelcome(); break;
            case "Devices": ShowDevices(); break;
            case "Inputs": ShowInputsOutputs(); break;
            case "Check": ShowValidation(); break;
            case "Scheme": ShowWorkspace(); break;
            case "Settings": ShowPlaceholder(T("Настройки", "Settings"), T("Здесь будут язык, внешний вид, лицензия и параметры программы.", "Language, appearance, license and application settings will be available here.")); break;
        }
    }

    private void ShowWelcome()
    {
        SetHeader(T("Добро пожаловать в Neutrivox", "Welcome to Neutrivox"), T("Создайте единый проект и настройте оборудование, логику и схему.", "Create one unified project and configure equipment, logic and diagram."));
        PageContent.Children.Clear();
        AddAction(T("Создать проект", "Create project"), T("Начните новую конфигурацию автоматизации", "Start a new automation configuration"), (_, _) => NewProjectButton_OnClick(null, null));
        AddAction(T("Открыть рабочее пространство", "Open workspace"), T("Посмотрите оборудование и связи проекта на общей схеме", "View project equipment and connections on one diagram"), (_, _) => ShowWorkspace());
    }

    private void ShowDevices()
    {
        SetHeader(T("Оборудование", "Equipment"), _project is null ? T("Сначала создайте проект, затем добавьте оборудование.", "Create a project first, then add equipment to it.") : T("Выберите устройство из каталога. Добавленные устройства сразу появляются в проекте и на схеме.", "Choose a device from the catalog. Added devices immediately appear in the project and workspace."));
        PageContent.Children.Clear();
        if (_project is null) { AddAction(T("Создать проект", "Create project"), T("Оборудование добавляется в единый проект", "Equipment is added to the unified project"), (_, _) => NewProjectButton_OnClick(null, null)); return; }

        PageContent.Children.Add(new TextBlock { Text = T("Каталог оборудования", "Equipment catalog"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        foreach (var definition in _catalog.Devices)
        {
            var button = new Button { HorizontalContentAlignment = HorizontalAlignment.Stretch, Margin = new Avalonia.Thickness(0, 2) };
            var text = new StackPanel { Spacing = 3, Margin = new Avalonia.Thickness(8) };
            text.Children.Add(new TextBlock { Text = definition.Model, FontWeight = Avalonia.Media.FontWeight.SemiBold });
            text.Children.Add(new TextBlock { Text = $"{definition.Manufacturer} • {definition.Category} • {definition.Channels.Count} I/O", Opacity = 0.65 });
            button.Content = text; button.Click += (_, _) => AddDevice(definition); PageContent.Children.Add(button);
        }

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock { Text = T("Состав текущего проекта", "Current project equipment"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (_project.Devices.Count == 0) PageContent.Children.Add(new TextBlock { Text = T("Пока нет добавленного оборудования.", "No equipment has been added yet."), Opacity = 0.65 });
        else foreach (var device in _project.Devices.ToList()) AddProjectDeviceCard(device);
    }

    private void AddDevice(DeviceDefinition definition)
    {
        if (_project is null) return;
        _equipment.AddDevice(_project, definition);
        _workspace.EnsureDeviceLayout(_project);
        UpdateProjectPanel(); ShowDevices();
    }

    private void AddProjectDeviceCard(ProjectDevice device)
    {
        var card = new Border { Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8), Padding = new Avalonia.Thickness(12) };
        var panel = new StackPanel { Spacing = 6 };
        panel.Children.Add(new TextBlock { Text = device.Name, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        panel.Children.Add(new TextBlock { Text = T($"Каналов: {device.Channels.Count}", $"Channels: {device.Channels.Count}"), Opacity = 0.65 });
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var io = new Button { Content = T("Настроить I/O", "Configure I/O") }; io.Click += (_, _) => ShowInputsOutputs();
        var workspace = new Button { Content = T("На схеме", "Show on diagram") }; workspace.Click += (_, _) => { _workspace.Selection.SelectDevice(device.Id); ShowWorkspace(); };
        var remove = new Button { Content = T("Удалить", "Remove") }; remove.Click += (_, _) => { if (_project != null) _equipment.RemoveDevice(_project, device.Id); UpdateProjectPanel(); ShowDevices(); };
        actions.Children.Add(io); actions.Children.Add(workspace); actions.Children.Add(remove); panel.Children.Add(actions); card.Child = panel; PageContent.Children.Add(card);
    }

    private void ShowWorkspace()
    {
        SetHeader(T("Рабочее пространство", "Workspace"), T("Одна схема для всего проекта. Здесь оборудование и его связи представлены вместе.", "One diagram for the entire project. Equipment and its connections are shown together."));
        PageContent.Children.Clear();
        if (_project is null) { AddAction(T("Создать проект", "Create project"), T("Рабочее пространство создаётся для проекта", "The workspace is created for a project"), (_, _) => NewProjectButton_OnClick(null, null)); return; }
        _workspace.EnsureDeviceLayout(_project);
        if (_project.Devices.Count == 0) { PageContent.Children.Add(new TextBlock { Text = T("Добавьте оборудование, чтобы начать построение схемы.", "Add equipment to start building the diagram."), Opacity = 0.7 }); return; }

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 8) };
        var arrange = new Button { Content = T("Авторасположение", "Auto arrange") }; arrange.Click += (_, _) => { _workspace.LoadProject(_project); ShowWorkspace(); };
        var addConnection = new Button { Content = T("Добавить связь", "Add connection") }; addConnection.Click += (_, _) => AddNextConnection();
        toolbar.Children.Add(arrange); toolbar.Children.Add(addConnection); PageContent.Children.Add(toolbar);

        var deviceGrid = new UniformGrid { Columns = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(_project.Devices.Count))), HorizontalAlignment = HorizontalAlignment.Stretch };
        foreach (var device in _project.Devices)
        {
            var selected = _workspace.Selection.DeviceId == device.Id;
            var card = new Button { Margin = new Avalonia.Thickness(6), HorizontalContentAlignment = HorizontalAlignment.Stretch };
            var panel = new StackPanel { Spacing = 4, Margin = new Avalonia.Thickness(10) };
            panel.Children.Add(new TextBlock { Text = selected ? "● " + device.Name : device.Name, FontWeight = Avalonia.Media.FontWeight.SemiBold });
            panel.Children.Add(new TextBlock { Text = T($"I/O: {device.Channels.Count}", $"I/O: {device.Channels.Count}"), Opacity = 0.65 });
            panel.Children.Add(new TextBlock { Text = device.PhysicalBinding is null ? T("Цифровая модель", "Digital model") : T("Прибор сопоставлен", "Physical device bound"), Opacity = 0.65 });
            card.Content = panel; card.Click += (_, _) => { _workspace.Selection.SelectDevice(device.Id); ShowWorkspace(); }; deviceGrid.Children.Add(card);
        }
        PageContent.Children.Add(deviceGrid);

        PageContent.Children.Add(new TextBlock { Text = T("Соединения", "Connections"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 14, 0, 4) });
        if (_project.Connections.Count == 0) PageContent.Children.Add(new TextBlock { Text = T("Связи ещё не добавлены.", "No connections have been added yet."), Opacity = 0.65 });
        else foreach (var connection in _project.Connections)
        {
            var from = _project.Devices.FirstOrDefault(x => x.Id == connection.FromDeviceId)?.Name ?? "?";
            var to = _project.Devices.FirstOrDefault(x => x.Id == connection.ToDeviceId)?.Name ?? "?";
            PageContent.Children.Add(new TextBlock { Text = $"{from} → {to} ({connection.Interface})", Margin = new Avalonia.Thickness(4) });
        }

        if (_workspace.Selection.DeviceId is Guid selectedId)
        {
            var selected = _project.Devices.FirstOrDefault(x => x.Id == selectedId);
            if (selected is not null) ShowSelectedDeviceProperties(selected);
        }
    }

    private void AddNextConnection()
    {
        if (_project is null || _project.Devices.Count < 2) return;
        var existing = _project.Connections.Select(x => (x.FromDeviceId, x.ToDeviceId)).ToHashSet();
        for (var i = 0; i < _project.Devices.Count; i++)
        for (var j = i + 1; j < _project.Devices.Count; j++)
        {
            var pair = (_project.Devices[i].Id, _project.Devices[j].Id);
            if (existing.Contains(pair)) continue;
            _connections.AddConnection(_project, pair.Item1, pair.Item2, "Project link");
            ShowWorkspace(); return;
        }
    }

    private void ShowSelectedDeviceProperties(ProjectDevice device)
    {
        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 14, 0, 8) });
        PageContent.Children.Add(new TextBlock { Text = T("Свойства выбранного устройства", "Selected device properties"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        var name = new TextBox { Text = device.Name, Watermark = T("Имя устройства", "Device name") }; name.LostFocus += (_, _) => { device.Name = name.Text ?? device.Name; UpdateProjectPanel(); }; PageContent.Children.Add(name);
        PageContent.Children.Add(new TextBlock { Text = T($"Каналов: {device.Channels.Count}", $"Channels: {device.Channels.Count}"), Opacity = 0.65 });
        PageContent.Children.Add(new TextBlock { Text = device.PhysicalBinding is null ? T("Физический прибор пока не привязан.", "No physical device is bound yet.") : $"{T("Адрес", "Endpoint")}: {device.PhysicalBinding.Endpoint}", Opacity = 0.65 });
    }

    private void ShowInputsOutputs()
    {
        SetHeader(T("Входы и выходы", "Inputs and outputs"), T("Настройка каналов добавленного оборудования.", "Configure channels of the added equipment.")); PageContent.Children.Clear();
        if (_project is null || _project.Devices.Count == 0) { AddAction(T("Добавить оборудование", "Add equipment"), T("Сначала добавьте хотя бы одно устройство", "Add at least one device first"), (_, _) => ShowDevices()); return; }
        foreach (var device in _project.Devices)
        {
            PageContent.Children.Add(new TextBlock { Text = device.Name, FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 8, 0, 2) });
            foreach (var channel in device.Channels)
            {
                var row = new Grid { ColumnDefinitions = new ColumnDefinitions("120,110,*"), Margin = new Avalonia.Thickness(0, 2) };
                row.Children.Add(new TextBlock { Text = channel.Name, VerticalAlignment = VerticalAlignment.Center });
                var type = new TextBlock { Text = $"{channel.Type} / {channel.Direction}", VerticalAlignment = VerticalAlignment.Center, Opacity = 0.65 }; Grid.SetColumn(type, 1); row.Children.Add(type);
                var description = new TextBox { Text = channel.Description ?? string.Empty, Watermark = T("Описание канала", "Channel description") }; description.LostFocus += (_, _) => channel.Description = description.Text; Grid.SetColumn(description, 2); row.Children.Add(description); PageContent.Children.Add(row);
            }
        }
    }

    private void ShowValidation()
    {
        SetHeader(T("Проверка проекта", "Project validation"), T("Базовая проверка конфигурации перед дальнейшей работой.", "Basic configuration validation before further work.")); PageContent.Children.Clear();
        if (_project is null) { PageContent.Children.Add(new TextBlock { Text = T("Проект ещё не создан.", "No project has been created yet.") }); return; }
        if (_project.Devices.Count == 0) PageContent.Children.Add(new TextBlock { Text = "⚠ " + T("В проект не добавлено оборудование.", "No equipment has been added to the project.") });
        else PageContent.Children.Add(new TextBlock { Text = "✓ " + T("Базовая структура проекта корректна. Следующие проверки будут расширяться вместе с редактором.", "The basic project structure is valid. Further checks will grow with the editor.") });
    }

    private void ShowPlaceholder(string title, string description) { SetHeader(title, description); PageContent.Children.Clear(); PageContent.Children.Add(new TextBlock { Text = T("Раздел находится в активной разработке.", "This section is under active development."), Opacity = 0.65 }); }
    private void AddAction(string title, string description, EventHandler<RoutedEventArgs> action) { var button = new Button { HorizontalContentAlignment = HorizontalAlignment.Stretch, Margin = new Avalonia.Thickness(0, 4) }; var panel = new StackPanel { Spacing = 4, Margin = new Avalonia.Thickness(10) }; panel.Children.Add(new TextBlock { Text = title, FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold }); panel.Children.Add(new TextBlock { Text = description, Opacity = 0.65, TextWrapping = Avalonia.Media.TextWrapping.Wrap }); button.Content = panel; button.Click += action; PageContent.Children.Add(button); }
    private void SetHeader(string title, string description) { PageTitle.Text = title; PageDescription.Text = description; }
    private string T(string ru, string en) => _english ? en : ru;
    private void UpdateProjectPanel() { ProjectNameText.Text = _project?.Name ?? T("Проект не создан", "No project created"); DeviceCountText.Text = T($"Оборудование: {_project?.Devices.Count ?? 0}", $"Equipment: {_project?.Devices.Count ?? 0}"); }
    private void LanguageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _english = !_english; LanguageButton.Content = _english ? "EN" : "RU"; SubtitleText.Text = T("Среда промышленной автоматизации", "Industrial automation environment"); NewProjectButton.Content = T("Новый проект", "New project"); NavigationTitle.Text = T("Навигация", "Navigation"); ProjectsNav.Content = T("📁  Проекты", "📁  Projects"); DevicesNav.Content = T("▣  Оборудование", "▣  Equipment"); SchemeNav.Content = T("⌁  Схема", "⌁  Workspace"); InputsNav.Content = T("⇄  Входы и выходы", "⇄  Inputs and outputs"); CheckNav.Content = T("✓  Проверка", "✓  Validation"); SettingsNav.Content = T("⚙  Настройки", "⚙  Settings"); ProjectPanelTitle.Text = T("Текущий проект", "Current project"); QuickStartTitle.Text = T("Быстрый старт", "Quick start"); QuickStartText.Text = T("1. Создайте проект\n2. Добавьте оборудование\n3. Настройте входы и выходы\n4. Откройте рабочее пространство", "1. Create a project\n2. Add equipment\n3. Configure I/O\n4. Open workspace"); UpdateProjectPanel(); ShowPage("Scheme");
    }
}
