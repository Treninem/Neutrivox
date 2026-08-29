using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly SimulationSessionService _simulationSessions = new();
    private readonly SimulationWorkflowService _simulationWorkflow = new();
    private readonly SimulationTraceService _simulationTrace = new();
    private SimulationSession? _simulationSession;
    private SimulationTrace? _simulationLog;

    private void SimulationNavigationButton_OnClick(object? sender, RoutedEventArgs e) => ShowSimulation();

    private void ShowSimulation()
    {
        SetHeader(
            T("Симуляция", "Simulation"),
            T("Проверяйте проект и логику без подключения физического контроллера. Все значения здесь виртуальные.",
              "Test the project and logic without a physical controller. All values here are virtual."));
        PageContent.Children.Clear();

        if (_project is null)
        {
            AddAction(T("Создать проект", "Create project"),
                T("Симуляция использует тот же проект, что схема и оборудование.", "Simulation uses the same project as the diagram and equipment."),
                (_, _) => NewProjectButton_OnClick(null, null));
            return;
        }

        EnsureSimulationSession();

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var start = new Button { Content = T("Запустить", "Start") };
        start.Click += (_, _) => { _simulationSessions.Start(_simulationSession!); ShowSimulation(); };
        var pause = new Button { Content = T("Пауза", "Pause") };
        pause.Click += (_, _) => { _simulationSessions.Pause(_simulationSession!); ShowSimulation(); };
        var stop = new Button { Content = T("Стоп", "Stop") };
        stop.Click += (_, _) => { _simulationSessions.Stop(_simulationSession!); ShowSimulation(); };
        var cycle = new Button { Content = T("Выполнить цикл", "Run cycle") };
        cycle.Click += (_, _) => RunSimulationCycle();
        var clear = new Button { Content = T("Очистить журнал", "Clear trace") };
        clear.Click += (_, _) => { _simulationLog?.Clear(); ShowSimulation(); };
        toolbar.Children.Add(start); toolbar.Children.Add(pause); toolbar.Children.Add(stop); toolbar.Children.Add(cycle); toolbar.Children.Add(clear);
        PageContent.Children.Add(toolbar);

        var state = new Border { Padding = new Avalonia.Thickness(12), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
        var statePanel = new StackPanel { Spacing = 4 };
        statePanel.Children.Add(new TextBlock { Text = T("Состояние симуляции", "Simulation state"), FontWeight = Avalonia.Media.FontWeight.SemiBold });
        statePanel.Children.Add(new TextBlock { Text = $"{T("Состояние", "State")}: {_simulationSession!.State}" });
        statePanel.Children.Add(new TextBlock { Text = $"{T("Цикл", "Cycle")}: {_simulationSession.Cycle}" });
        statePanel.Children.Add(new TextBlock { Text = $"{T("Событий", "Events")}: {_simulationSession.Events.Count}" });
        state.Child = statePanel;
        PageContent.Children.Add(state);

        PageContent.Children.Add(new TextBlock { Text = T("Виртуальные входы", "Virtual inputs"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 10, 0, 4) });
        foreach (var device in _project.Devices)
            foreach (var channel in device.Channels.Where(x => x.Direction.Equals("Input", StringComparison.OrdinalIgnoreCase)))
                AddVirtualInput(device, channel);

        PageContent.Children.Add(new TextBlock { Text = T("Виртуальные выходы", "Virtual outputs"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 10, 0, 4) });
        foreach (var device in _project.Devices)
            foreach (var channel in device.Channels.Where(x => x.Direction.Equals("Output", StringComparison.OrdinalIgnoreCase)))
            {
                _simulationSession.ChannelValues.TryGetValue(channel.Id, out var value);
                PageContent.Children.Add(new TextBlock { Text = $"{device.Name} / {channel.Name}: {FormatValue(value)}", Margin = new Avalonia.Thickness(4) });
            }

        PageContent.Children.Add(new TextBlock { Text = T("Журнал симуляции", "Simulation trace"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 10, 0, 4) });
        var entries = _simulationLog?.Entries.TakeLast(100).ToList() ?? [];
        if (entries.Count == 0)
            PageContent.Children.Add(new TextBlock { Text = T("Событий пока нет.", "No trace events yet."), Opacity = 0.65 });
        else
            foreach (var entry in entries)
                PageContent.Children.Add(new TextBlock { Text = $"{entry.TimestampUtc.ToLocalTime():HH:mm:ss} [{entry.Category}] {entry.Message}", TextWrapping = Avalonia.Media.TextWrapping.Wrap });
    }

    private void EnsureSimulationSession()
    {
        if (_simulationSession?.ProjectId == _project!.Id) return;
        _simulationSession = _simulationSessions.Create(_project);
        _simulationLog = _simulationTrace.Create();
        _simulationTrace.RecordValidation(_simulationLog, new ProjectValidationWorkflowService().ValidateForSimulation(_project));
    }

    private void AddVirtualInput(ProjectDevice device, IoChannel channel)
    {
        _simulationSession!.ChannelValues.TryGetValue(channel.Id, out var value);
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        row.Children.Add(new TextBlock { Text = $"{device.Name} / {channel.Name}", Width = 220, VerticalAlignment = VerticalAlignment.Center });
        if (channel.Type.Equals("Digital", StringComparison.OrdinalIgnoreCase))
        {
            var toggle = new CheckBox { IsChecked = value is true, Content = T("ВКЛ", "ON") };
            toggle.Click += (_, _) => { _simulationSessions.SetChannelValue(_simulationSession!, _project!, channel.Id, toggle.IsChecked == true); ShowSimulation(); };
            row.Children.Add(toggle);
        }
        else
        {
            var numeric = new NumericUpDown { Value = value is null ? 0 : Convert.ToDecimal(value), Minimum = -100000, Maximum = 100000, Increment = 1, Width = 140 };
            numeric.ValueChanged += (_, _) => { if (numeric.Value is decimal number) _simulationSessions.SetChannelValue(_simulationSession!, _project!, channel.Id, (double)number); };
            row.Children.Add(numeric);
        }
        PageContent.Children.Add(row);
    }

    private void RunSimulationCycle()
    {
        if (_project is null || _simulationSession is null || _simulationLog is null) return;
        _simulationSessions.Start(_simulationSession);
        var result = _simulationWorkflow.RunCycle(_project, _simulationSession);
        _simulationTrace.RecordExecution(_simulationLog, new LogicExecutionResult(result.Success, result.ExecutedInstructions, result.Errors));
        foreach (var entry in result.Trace.Entries)
            if (!_simulationLog.Entries.Contains(entry)) _simulationLog.Entries.Add(entry);
        ShowSimulation();
    }

    private static string FormatValue(object? value) => value switch
    {
        null => "—",
        bool boolean => boolean ? "ON" : "OFF",
        _ => Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture) ?? "—"
    };
}
