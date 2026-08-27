using Avalonia.Controls;
using Avalonia.Interactivity;
using Neutrivox.Models;

namespace Neutrivox;

public partial class MainWindow : Window
{
    private bool _english;
    private AutomationProject? _project;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void NewProjectButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _project = new AutomationProject { Name = _english ? "New project" : "Новый проект" };
        ProjectNameText.Text = _project.Name;
        StatusText.Text = _english ? "Project created. Add equipment to continue." : "Проект создан. Добавьте оборудование, чтобы продолжить.";
    }

    private void DevicesButton_OnClick(object? sender, RoutedEventArgs e) => ShowPage("Devices");
    private void CheckButton_OnClick(object? sender, RoutedEventArgs e) => ShowPage("Check");

    private void NavigationButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: string page }) ShowPage(page);
    }

    private void ShowPage(string page)
    {
        var ru = new Dictionary<string, (string Title, string Description)>
        {
            ["Projects"] = ("Проекты", "Создавайте, открывайте и сохраняйте проекты автоматизации."),
            ["Devices"] = ("Оборудование", "Выберите контроллеры и модули для текущего проекта."),
            ["Scheme"] = ("Схема", "Визуально размещайте устройства и создавайте соединения."),
            ["Inputs"] = ("Входы и выходы", "Настраивайте дискретные и аналоговые каналы."),
            ["Check"] = ("Проверка", "Проверьте конфигурацию перед работой с оборудованием."),
            ["Settings"] = ("Настройки", "Язык, внешний вид и параметры программы.")
        };
        var en = new Dictionary<string, (string Title, string Description)>
        {
            ["Projects"] = ("Projects", "Create, open and save automation projects."),
            ["Devices"] = ("Equipment", "Choose controllers and modules for the current project."),
            ["Scheme"] = ("Diagram", "Place devices visually and create connections."),
            ["Inputs"] = ("Inputs and outputs", "Configure digital and analog channels."),
            ["Check"] = ("Validation", "Check the configuration before working with equipment."),
            ["Settings"] = ("Settings", "Language, appearance and application options.")
        };
        var data = (_english ? en : ru).GetValueOrDefault(page, (_english ? "Neutrivox" : "Neutrivox", ""));
        PageTitle.Text = data.Title;
        PageDescription.Text = data.Description;
    }

    private void LanguageButton_OnClick(object? sender, RoutedEventArgs e)
    {
        _english = !_english;
        LanguageButton.Content = _english ? "EN" : "RU";
        SubtitleText.Text = _english ? "Industrial automation environment" : "Среда промышленной автоматизации";
        NewProjectButton.Content = _english ? "New project" : "Новый проект";
        NavigationTitle.Text = _english ? "Navigation" : "Навигация";
        ProjectsNav.Content = _english ? "📁  Projects" : "📁  Проекты";
        DevicesNav.Content = _english ? "▣  Equipment" : "▣  Оборудование";
        SchemeNav.Content = _english ? "⌁  Diagram" : "⌁  Схема";
        InputsNav.Content = _english ? "⇄  Inputs and outputs" : "⇄  Входы и выходы";
        CheckNav.Content = _english ? "✓  Validation" : "✓  Проверка";
        SettingsNav.Content = _english ? "⚙  Settings" : "⚙  Настройки";
        CreateCardTitle.Text = _english ? "Create project" : "Создать проект";
        CreateCardText.Text = _english ? "Start a new automation diagram" : "Начните новую схему автоматизации";
        DevicesCardTitle.Text = _english ? "Add equipment" : "Добавить оборудование";
        DevicesCardText.Text = _english ? "Choose a controller or module" : "Выберите контроллер или модуль";
        CheckCardTitle.Text = _english ? "Validate project" : "Проверить проект";
        CheckCardText.Text = _english ? "Find errors before connecting equipment" : "Найдите ошибки до подключения оборудования";
        StatusTitle.Text = _english ? "Status" : "Статус";
        StatusText.Text = _english ? "The application is ready for the first project." : "Программа готова к созданию первого проекта.";
        ProjectPanelTitle.Text = _english ? "Current project" : "Текущий проект";
        ProjectNameText.Text = _project?.Name ?? (_english ? "No project created" : "Проект не создан");
        QuickStartTitle.Text = _english ? "Quick start" : "Быстрый старт";
        QuickStartText.Text = _english ? "1. Create a project\n2. Add equipment\n3. Configure I/O\n4. Validate configuration" : "1. Создайте проект\n2. Добавьте оборудование\n3. Настройте входы и выходы\n4. Проверьте конфигурацию";
        PageTitle.Text = _english ? "Welcome to Neutrivox" : "Добро пожаловать в Neutrivox";
        PageDescription.Text = _english ? "Create a project and start configuring equipment." : "Создайте проект и начните настройку оборудования.";
    }
}
