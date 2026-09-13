using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly LocalLicenseService _localLicense = new();
    private readonly LicensePolicyService _licensePolicy = new();
    private readonly ApplicationSettingsService _settingsService = new();
    private ApplicationSettings _settings = new();
    private LicenseRuntimeState? _licenseRuntime;
    private string? _licenseMessage;

    private void InitializeSettingsAndLicensing()
    {
        _settings = _settingsService.Load();
        _english = _settings.English;
        _licenseRuntime = _localLicense.GetCurrent(DateTimeOffset.UtcNow);
        ApplyLocalization();
    }

    private void RefreshLicenseState() => _licenseRuntime = _localLicense.GetCurrent(DateTimeOffset.UtcNow);

    private bool CanUsePhysicalDeviceIntegration()
    {
        RefreshLicenseState();
        return _licenseRuntime is not null &&
               _licensePolicy.CanUse(_licenseRuntime.Snapshot, DateTimeOffset.UtcNow, x => x.PhysicalDeviceIntegration);
    }

    private void ShowSettings()
    {
        RefreshLicenseState();
        SetHeader(T("Настройки и лицензия", "Settings & license"),
            T("Язык, лицензия, пробный период и внешние официальные инструменты.",
              "Language, license, trial and official external tools."));
        PageContent.Children.Clear();

        var language = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        language.Children.Add(new TextBlock { Text = T("Язык интерфейса", "Interface language"), Width = 180, VerticalAlignment = VerticalAlignment.Center });
        var languageButton = new Button { Content = _english ? "Русский" : "English" };
        languageButton.Click += (_, _) => ToggleLanguage();
        language.Children.Add(languageButton);
        PageContent.Children.Add(language);

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 8) });
        PageContent.Children.Add(new TextBlock { Text = T("Лицензия", "License"), FontSize = 19, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (_licenseRuntime is not null)
        {
            PageContent.Children.Add(new TextBlock
            {
                Text = $"{T("Редакция", "Edition")}: {_licenseRuntime.Snapshot.Edition} • {_licenseRuntime.Snapshot.State}",
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            });
            PageContent.Children.Add(new TextBlock
            {
                Text = _english ? _licenseRuntime.StatusEn : _licenseRuntime.StatusRu,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Opacity = 0.8
            });
            if (_licenseRuntime.Snapshot.ExpiresAtUtc is DateTimeOffset expiration)
                PageContent.Children.Add(new TextBlock { Text = $"{T("Действует до", "Valid until")}: {expiration.ToLocalTime():g}", Opacity = 0.7 });

            PageContent.Children.Add(new TextBlock
            {
                Text = T(
                    "Отпечаток этого компьютера для покупки привязанного ключа:",
                    "This computer fingerprint for a device-bound license:"),
                Margin = new Avalonia.Thickness(0, 6, 0, 2),
                Opacity = 0.75
            });
            PageContent.Children.Add(new TextBox
            {
                Text = _licenseRuntime.Fingerprint,
                IsReadOnly = true,
                TextWrapping = Avalonia.Media.TextWrapping.Wrap
            });
            PageContent.Children.Add(new TextBlock
            {
                Text = T(
                    "Передайте продавцу только этот fingerprint. Пароли и другие данные для выпуска ключа не нужны.",
                    "Send only this fingerprint to the seller. Passwords or other personal data are not required to issue a key."),
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Opacity = 0.55
            });
        }

        PageContent.Children.Add(new TextBlock { Text = T("Активация ключа", "License activation"), FontSize = 17, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 8, 0, 0) });
        var keyBox = new TextBox
        {
            AcceptsReturn = true,
            MinHeight = 90,
            Watermark = T("Вставьте подписанный ключ лицензии", "Paste the signed license key"),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        PageContent.Children.Add(keyBox);
        var activate = new Button { Content = T("Активировать", "Activate"), HorizontalAlignment = HorizontalAlignment.Left };
        activate.Click += (_, _) =>
        {
            var result = _localLicense.Activate(keyBox.Text ?? string.Empty, DateTimeOffset.UtcNow);
            _licenseMessage = _english ? result.MessageEn : result.MessageRu;
            RefreshLicenseState();
            ShowSettings();
        };
        PageContent.Children.Add(activate);
        if (!string.IsNullOrWhiteSpace(_licenseMessage))
            PageContent.Children.Add(new TextBlock { Text = _licenseMessage, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock { Text = T("Тарифы", "Plans"), FontSize = 19, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        foreach (var plan in new CommercialPlanCatalogService().GetPublicPlans())
        {
            var price = plan.PriceRub == 0 ? T("Бесплатно", "Free") : $"{plan.PriceRub:0} ₽";
            var duration = plan.DurationDays is null ? string.Empty : $" • {plan.DurationDays} {T("дн.", "days")}";
            PageContent.Children.Add(new TextBlock
            {
                Text = $"{(_english ? plan.NameEn : plan.NameRu)} — {price}{duration}\n{(_english ? plan.DescriptionEn : plan.DescriptionRu)}",
                TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                Margin = new Avalonia.Thickness(0, 3)
            });
        }

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock { Text = T("ОВЕН: официальная утилита передачи", "OWEN: official transfer utility"), FontSize = 19, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        PageContent.Children.Add(new TextBlock
        {
            Text = T(
                "Neutrivox не подменяет протокол загрузки. Для поддерживаемых и аппаратно проверенных ПР используется официальная Owen Logic Replication Utility.",
                "Neutrivox does not invent a programming protocol. Supported and hardware-verified PR devices use the official Owen Logic Replication Utility."),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Opacity = 0.7
        });
        var utilityPath = new TextBox
        {
            Text = _settings.OwenReplicationUtilityPath ?? string.Empty,
            Watermark = T("Путь к официальной утилите .exe", "Path to the official utility .exe")
        };
        utilityPath.LostFocus += (_, _) =>
        {
            _settings.OwenReplicationUtilityPath = string.IsNullOrWhiteSpace(utilityPath.Text) ? null : utilityPath.Text.Trim();
            _settingsService.Save(_settings);
        };
        PageContent.Children.Add(utilityPath);
        var browse = new Button { Content = T("Выбрать файл…", "Browse…"), HorizontalAlignment = HorizontalAlignment.Left };
        browse.Click += async (_, _) =>
        {
            var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
            {
                Title = T("Выберите официальную утилиту ОВЕН", "Select the official OWEN utility"),
                AllowMultiple = false,
                FileTypeFilter = [new FilePickerFileType("Windows executable") { Patterns = ["*.exe"] }]
            });
            var path = files.FirstOrDefault()?.TryGetLocalPath();
            if (!string.IsNullOrWhiteSpace(path))
            {
                _settings.OwenReplicationUtilityPath = path;
                _settingsService.Save(_settings);
                ShowSettings();
            }
        };
        PageContent.Children.Add(browse);

        PageContent.Children.Add(new TextBlock
        {
            Text = $"Neutrivox {GetProductVersion()}",
            Opacity = 0.5,
            Margin = new Avalonia.Thickness(0, 14, 0, 0)
        });
    }

    private void ToggleLanguage()
    {
        _english = !_english;
        _settings.English = _english;
        _settingsService.Save(_settings);
        ApplyLocalization();
        ShowSettings();
    }

    private void ApplyLocalization()
    {
        SubtitleText.Text = T("Среда промышленной автоматизации", "Industrial automation environment");
        NavigationTitle.Text = T("Навигация", "Navigation");
        ProjectsNav.Content = T("📁  Проекты", "📁  Projects");
        DevicesNav.Content = T("▣  Оборудование", "▣  Equipment");
        ConnectionNav.Content = T("↔  Подключение", "↔  Connection");
        SchemeNav.Content = T("⌁  Схема", "⌁  Diagram");
        LogicNav.Content = T("⚡  Логика", "⚡  Logic");
        SimulationNav.Content = T("▶  Симуляция", "▶  Simulation");
        InputsNav.Content = T("⇄  Входы и выходы", "⇄  Inputs & outputs");
        CheckNav.Content = T("✓  Проверка", "✓  Validation");
        SettingsNav.Content = T("⚙  Настройки", "⚙  Settings");
        ProjectPanelTitle.Text = T("Текущий проект", "Current project");
        QuickStartTitle.Text = T("Быстрый старт", "Quick start");
        QuickStartText.Text = T(
            "1. Создайте/откройте проект\n2. Добавьте оборудование\n3. Настройте I/O\n4. Создайте логику\n5. Запустите симуляцию\n6. Проверьте проект",
            "1. Create/open a project\n2. Add equipment\n3. Configure I/O\n4. Create logic\n5. Run simulation\n6. Validate the project");
        NewProjectButton.Content = T("Новый проект", "New project");
        OpenProjectButton.Content = T("Открыть", "Open");
        SaveProjectButton.Content = T("Сохранить", "Save");
        LanguageButton.Content = _english ? "RU" : "EN";
        UpdateProjectPanel();
    }

    private static string GetProductVersion() =>
        typeof(MainWindow).Assembly.GetName().Version?.ToString(3) ?? "0.0.0";
}
