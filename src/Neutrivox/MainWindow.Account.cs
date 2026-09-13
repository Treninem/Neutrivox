using Avalonia.Controls;
using Avalonia.Layout;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly AccountLicenseClient _accountClient = new();
    private AccountSessionInfo? _accountSession;
    private string? _accountMessage;

    private LicenseSnapshot GetEffectiveLicenseSnapshot()
    {
        RefreshLicenseState();
        if (_accountSession is { Success: true } session &&
            Enum.TryParse<ProductEdition>(session.Edition, true, out var edition) &&
            (session.ExpiresAtUtc is null || session.ExpiresAtUtc > DateTimeOffset.UtcNow))
        {
            return new LicenseSnapshot(edition, LicenseState.Active, session.ExpiresAtUtc, "Account " + session.PlanId);
        }
        return _licenseRuntime?.Snapshot ?? new LicenseSnapshot(ProductEdition.Free, LicenseState.Active, null, "Free");
    }

    private void AddAccountSettingsSection()
    {
        if (_licenseRuntime is null) RefreshLicenseState();
        var fingerprint = _licenseRuntime?.Fingerprint ?? new DeviceFingerprintService().GetFingerprint();

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 12) });
        PageContent.Children.Add(new TextBlock
        {
            Text = T("Аккаунт — работа на нескольких своих ПК", "Account — use on your own PCs"),
            FontSize = 19,
            FontWeight = Avalonia.Media.FontWeight.SemiBold
        });
        PageContent.Children.Add(new TextBlock
        {
            Text = T(
                "Ключ покупки можно один раз привязать к аккаунту. После этого вход на другом своём ПК выполняется по аккаунту; число активных устройств ограничено тарифом.",
                "A purchase key can be linked to an account once. You can then sign in on another PC you own; active-device count is limited by the plan."),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Opacity = 0.7
        });

        var serverUrl = new TextBox { Text = _settings.LicenseServerUrl, Watermark = "https://license.example.com" };
        serverUrl.LostFocus += (_, _) =>
        {
            _settings.LicenseServerUrl = serverUrl.Text?.Trim() ?? string.Empty;
            _settingsService.Save(_settings);
        };
        PageContent.Children.Add(new TextBlock { Text = T("HTTPS-адрес сервера лицензий", "License server HTTPS URL"), Opacity = 0.7 });
        PageContent.Children.Add(serverUrl);

        var email = new TextBox { Text = _settings.AccountEmail ?? string.Empty, Watermark = "email@example.com" };
        var password = new TextBox { PasswordChar = '●', Watermark = T("Пароль (минимум 12 символов)", "Password (minimum 12 characters)") };
        PageContent.Children.Add(email);
        PageContent.Children.Add(password);

        var purchaseKey = new TextBox
        {
            AcceptsReturn = true,
            MinHeight = 70,
            Watermark = T("Ключ покупки — нужен только при создании аккаунта", "Purchase key — required only when creating the account"),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap
        };
        PageContent.Children.Add(purchaseKey);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var register = new Button { Content = T("Создать аккаунт", "Create account") };
        register.Click += async (_, _) =>
        {
            PersistAccountInputs(serverUrl, email);
            var result = await _accountClient.RegisterAsync(
                _settings.LicenseServerUrl,
                new AccountRegisterRequest(email.Text ?? string.Empty, password.Text ?? string.Empty, purchaseKey.Text ?? string.Empty, fingerprint, Environment.MachineName));
            ApplyAccountResult(result);
            ShowSettings();
        };
        var login = new Button { Content = T("Войти", "Sign in") };
        login.Click += async (_, _) =>
        {
            PersistAccountInputs(serverUrl, email);
            var result = await _accountClient.LoginAsync(
                _settings.LicenseServerUrl,
                new AccountLoginRequest(email.Text ?? string.Empty, password.Text ?? string.Empty, fingerprint, Environment.MachineName));
            ApplyAccountResult(result);
            ShowSettings();
        };
        actions.Children.Add(register); actions.Children.Add(login);

        if (_accountSession is { Success: true, SessionToken: not null })
        {
            var refresh = new Button { Content = T("Обновить", "Refresh") };
            refresh.Click += async (_, _) =>
            {
                var result = await _accountClient.StatusAsync(_settings.LicenseServerUrl, _accountSession.SessionToken!);
                ApplyAccountResult(result, preserveToken: true);
                ShowSettings();
            };
            var logout = new Button { Content = T("Выйти", "Sign out") };
            logout.Click += (_, _) => { _accountSession = null; _accountMessage = T("Выход выполнен.", "Signed out."); ShowSettings(); };
            actions.Children.Add(refresh); actions.Children.Add(logout);
        }
        PageContent.Children.Add(actions);

        if (!string.IsNullOrWhiteSpace(_accountMessage))
            PageContent.Children.Add(new TextBlock { Text = _accountMessage, TextWrapping = Avalonia.Media.TextWrapping.Wrap });

        if (_accountSession is { Success: true } active)
        {
            PageContent.Children.Add(new TextBlock
            {
                Text = $"{T("План", "Plan")}: {active.PlanId} • {active.Edition} • {T("устройств", "devices")}: {active.Devices.Count}/{active.DeviceLimit}",
                FontWeight = Avalonia.Media.FontWeight.SemiBold
            });
            if (active.ExpiresAtUtc is DateTimeOffset expires)
                PageContent.Children.Add(new TextBlock { Text = $"{T("Действует до", "Valid until")}: {expires.ToLocalTime():g}" });

            foreach (var device in active.Devices)
            {
                var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
                var current = string.Equals(device.Fingerprint, fingerprint, StringComparison.Ordinal);
                row.Children.Add(new TextBlock
                {
                    Text = $"{(current ? "●" : "○")} {device.Name} — {device.Fingerprint[..Math.Min(12, device.Fingerprint.Length)]}…",
                    VerticalAlignment = VerticalAlignment.Center
                });
                if (!current && active.SessionToken is not null)
                {
                    var revoke = new Button { Content = T("Отключить", "Revoke") };
                    revoke.Click += async (_, _) =>
                    {
                        var result = await _accountClient.RevokeDeviceAsync(_settings.LicenseServerUrl, active.SessionToken, device.Fingerprint);
                        ApplyAccountResult(result, preserveToken: true);
                        ShowSettings();
                    };
                    row.Children.Add(revoke);
                }
                PageContent.Children.Add(row);
            }
        }
    }

    private void PersistAccountInputs(TextBox serverUrl, TextBox email)
    {
        _settings.LicenseServerUrl = serverUrl.Text?.Trim() ?? string.Empty;
        _settings.AccountEmail = email.Text?.Trim();
        _settingsService.Save(_settings);
    }

    private void ApplyAccountResult(AccountSessionInfo result, bool preserveToken = false)
    {
        if (result.Success && preserveToken && string.IsNullOrWhiteSpace(result.SessionToken) && _accountSession?.SessionToken is string token)
            result = result with { SessionToken = token };
        _accountMessage = result.Message;
        if (result.Success) _accountSession = result;
    }
}
