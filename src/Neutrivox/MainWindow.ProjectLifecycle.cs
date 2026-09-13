using Avalonia.Controls;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly ProjectFileService _projectFiles = new();
    private readonly PersistentProjectRecoveryService _persistentRecovery = new();
    private DispatcherTimer? _autosaveTimer;
    private string? _projectPath;
    private string? _projectMessage;

    private void InitializeProjectLifecycle()
    {
        _autosaveTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
        _autosaveTimer.Tick += async (_, _) => await CaptureRecoveryAsync();
        _autosaveTimer.Start();
        Closing += async (_, _) => await CaptureRecoveryAsync();
    }

    private async void OpenProjectButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = T("Открыть проект Neutrivox", "Open Neutrivox project"),
            AllowMultiple = false,
            FileTypeFilter = [ProjectFileType()]
        });
        var file = files.FirstOrDefault();
        var path = file?.TryGetLocalPath();
        if (string.IsNullOrWhiteSpace(path)) return;

        try
        {
            var project = await _projectFiles.LoadAsync(path);
            var integrity = new ProjectIntegrityService().Check(project);
            if (!integrity.IsValid)
            {
                _projectMessage = T("Файл проекта не прошёл проверку целостности.", "The project file failed integrity validation.");
                ShowWelcome();
                return;
            }
            LoadProject(project, path);
            _projectMessage = T("Проект открыт.", "Project opened.");
            await CaptureRecoveryAsync();
            ShowWelcome();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or System.Text.Json.JsonException)
        {
            _projectMessage = T("Не удалось открыть проект: ", "Could not open project: ") + ex.Message;
            ShowWelcome();
        }
    }

    private async void SaveProjectButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await SaveProjectAsync(saveAs: false);

    private async void SaveProjectAsButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        => await SaveProjectAsync(saveAs: true);

    private async Task SaveProjectAsync(bool saveAs)
    {
        if (_project is null)
        {
            _projectMessage = T("Сначала создайте или откройте проект.", "Create or open a project first.");
            ShowWelcome();
            return;
        }

        var path = saveAs ? null : _projectPath;
        if (string.IsNullOrWhiteSpace(path))
        {
            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = T("Сохранить проект Neutrivox", "Save Neutrivox project"),
                SuggestedFileName = SafeProjectFileName(_project.Name) + ".neutrivox",
                DefaultExtension = "neutrivox",
                FileTypeChoices = [ProjectFileType()]
            });
            path = file?.TryGetLocalPath();
        }
        if (string.IsNullOrWhiteSpace(path)) return;
        if (!_projectFiles.IsProjectFile(path)) path += ".neutrivox";

        try
        {
            await _projectFiles.SaveAsync(_project, path);
            _projectPath = path;
            _projectMessage = T("Проект сохранён: ", "Project saved: ") + Path.GetFileName(path);
            await CaptureRecoveryAsync();
            UpdateProjectPanel();
            ShowWelcome();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            _projectMessage = T("Не удалось сохранить проект: ", "Could not save project: ") + ex.Message;
            ShowWelcome();
        }
    }

    private async Task CaptureRecoveryAsync()
    {
        if (_project is null) return;
        try { await _persistentRecovery.SaveAsync(_project, _projectPath); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { _ = ex; }
    }

    private async void RestoreLatestRecovery()
    {
        var result = await _persistentRecovery.LoadLatestAsync();
        if (!result.Success || result.Entry is null)
        {
            _projectMessage = T("Точка восстановления недоступна.", "Recovery snapshot is unavailable.");
            ShowWelcome();
            return;
        }
        var integrity = new ProjectIntegrityService().Check(result.Entry.Project);
        if (!integrity.IsValid)
        {
            _projectMessage = T("Точка восстановления повреждена.", "Recovery snapshot is invalid.");
            ShowWelcome();
            return;
        }
        LoadProject(result.Entry.Project, result.Entry.SourcePath);
        _projectMessage = T(
            $"Восстановлен снимок от {result.Entry.SavedAtUtc.ToLocalTime():g}.",
            $"Restored snapshot from {result.Entry.SavedAtUtc.ToLocalTime():g}.");
        ShowWelcome();
    }

    private void LoadProject(AutomationProject project, string? path)
    {
        _project = project;
        _projectPath = path;
        _workspace.LoadProject(project);
        _simulationSession = null;
        _simulationLog = null;
        UpdateProjectPanel();
    }

    private void AddProjectLifecycleActions()
    {
        if (_project is not null)
        {
            AddAction(T("Сохранить проект", "Save project"),
                _projectPath is null ? T("Выберите файл .neutrivox", "Choose a .neutrivox file") : Path.GetFileName(_projectPath),
                async (_, _) => await SaveProjectAsync(false));
            AddAction(T("Сохранить как…", "Save as…"), T("Создать отдельную копию проекта", "Create a separate project copy"),
                async (_, _) => await SaveProjectAsync(true));
        }
        if (_persistentRecovery.HasRecovery)
            AddAction(T("Восстановить автосохранение", "Restore autosave"),
                T("Открыть последний аварийный/автоматический снимок", "Open the latest automatic recovery snapshot"),
                (_, _) => RestoreLatestRecovery());
        if (!string.IsNullOrWhiteSpace(_projectMessage))
            PageContent.Children.Add(new TextBlock { Text = _projectMessage, TextWrapping = Avalonia.Media.TextWrapping.Wrap, Opacity = 0.8 });
    }

    private static FilePickerFileType ProjectFileType() => new("Neutrivox project")
    {
        Patterns = ["*.neutrivox"],
        MimeTypes = ["application/json"]
    };

    private static string SafeProjectFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        var safe = new string(name.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray()).Trim();
        return string.IsNullOrWhiteSpace(safe) ? "project" : safe;
    }
}
