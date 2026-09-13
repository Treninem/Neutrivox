using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Neutrivox.Models;
using Neutrivox.Services;

namespace Neutrivox;

public partial class MainWindow
{
    private readonly LogicEditorWorkflowService _logicWorkflow = new();
    private readonly LogicEditorPresenterService _logicPresenter = new();
    private readonly ProjectSummaryService _projectSummary = new();
    private readonly LogicProjectService _logicProjectOperations = new();
    private readonly StructuredTextCompilerService _stCompiler = new();
    private string? _stCompileMessage;

    private void LogicNavigationButton_OnClick(object? sender, RoutedEventArgs e) => ShowLogicEditor();

    private void ShowLogicEditor()
    {
        SetHeader(
            T("Логика контроллера", "Controller logic"),
            T("Одна логическая модель проекта доступна как FBD, Ladder/LD и Structured Text. Симуляция использует общий безопасный IR.",
              "One project logic model is available as FBD, Ladder/LD and Structured Text. Simulation uses the same safe IR."));
        PageContent.Children.Clear();

        if (_project is null)
        {
            AddAction(T("Создать проект", "Create project"),
                T("Редактор логики работает внутри единого проекта Neutrivox.", "The logic editor works inside the unified Neutrivox project."),
                (_, _) => NewProjectButton_OnClick(null, null!));
            return;
        }

        AddLogicModeToolbar();
        if (_project.Logic.EditorMode == LogicEditorMode.StructuredText)
        {
            ShowStructuredTextEditor();
            return;
        }

        var view = _logicPresenter.Build(_project);
        var summary = _projectSummary.CreateHumanReadableSummary(_project);
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 12) };
        var newNetwork = new Button { Content = T("+ Сеть", "+ Network") };
        newNetwork.Click += (_, _) => { _logicWorkflow.CreateNetwork(_project, T("Новая сеть", "New network")); _ = CaptureRecoveryAsync(); ShowLogicEditor(); };
        var newVariable = new Button { Content = T("+ Переменная", "+ Variable") };
        newVariable.Click += (_, _) => { _logicWorkflow.CreateVariable(_project, T("Переменная", "Variable"), TagDataType.Boolean); _ = CaptureRecoveryAsync(); ShowLogicEditor(); };
        var validate = new Button { Content = T("Проверить", "Validate") };
        validate.Click += (_, _) => ShowLogicValidation();
        toolbar.Children.Add(newNetwork); toolbar.Children.Add(newVariable); toolbar.Children.Add(validate);
        PageContent.Children.Add(toolbar);

        if (_project.Logic.EditorMode == LogicEditorMode.Fbd)
        {
            PageContent.Children.Add(new TextBlock { Text = T("FBD — функциональные блоки", "FBD — Function Block Diagram"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
            PageContent.Children.Add(new TextBlock { Text = T("Каждая операция представлена как блок: входы слева, функция в центре, результат справа.", "Each operation is shown as a block with inputs, function and output."), Opacity = 0.65, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        }
        else
        {
            PageContent.Children.Add(new TextBlock { Text = T("LD — лестничная логика", "LD — Ladder Diagram"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
            PageContent.Children.Add(new TextBlock { Text = T("Сети отображаются в виде rung-цепочек контактов и катушек.", "Networks are rendered as contact/coil rungs."), Opacity = 0.65 });
        }

        PageContent.Children.Add(new TextBlock { Text = T("Инструменты логики", "Logic toolbox"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 8, 0, 0) });
        foreach (var block in view.Toolbox)
        {
            var add = new Button { Content = $"{block.DisplayName} — {block.Description}", HorizontalContentAlignment = HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 2) };
            add.Click += (_, _) => AddLogicBlockToSelectedOrFirst(block.Kind);
            PageContent.Children.Add(add);
        }

        AddVariables(view);
        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock { Text = T("Логические сети", "Logic networks"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (view.Networks.Count == 0)
            PageContent.Children.Add(new TextBlock { Text = T("Создайте первую сеть кнопкой «+ Сеть».", "Create the first network with '+ Network'."), Opacity = 0.65 });
        else foreach (var network in view.Networks) AddNetworkCard(network);

        AddSymbolList(view);
        var status = new Border { Padding = new Avalonia.Thickness(12), Margin = new Avalonia.Thickness(0, 12, 0, 0), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
        var statusPanel = new StackPanel { Spacing = 4 };
        statusPanel.Children.Add(new TextBlock { Text = T("Готовность логики", "Logic readiness"), FontWeight = Avalonia.Media.FontWeight.SemiBold });
        statusPanel.Children.Add(new TextBlock { Text = view.Readiness.Success ? "✓ " + T("Готова к симуляции", "Ready for simulation") : "⚠ " + T("Найдены ошибки", "Errors detected") });
        statusPanel.Children.Add(new TextBlock { Text = summary, Opacity = 0.55, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        status.Child = statusPanel; PageContent.Children.Add(status);
    }

    private void AddLogicModeToolbar()
    {
        if (_project is null) return;
        var modes = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 12) };
        AddModeButton(modes, LogicEditorMode.Fbd, "FBD");
        AddModeButton(modes, LogicEditorMode.Ladder, "Ladder / LD");
        AddModeButton(modes, LogicEditorMode.StructuredText, "Structured Text / ST");
        PageContent.Children.Add(modes);
    }

    private void AddModeButton(StackPanel modes, LogicEditorMode mode, string label)
    {
        var button = new Button { Content = (_project!.Logic.EditorMode == mode ? "● " : string.Empty) + label };
        button.Click += (_, _) =>
        {
            if (mode == LogicEditorMode.StructuredText && string.IsNullOrWhiteSpace(_project.Logic.StructuredTextSource))
                _project.Logic.StructuredTextSource = _stCompiler.Render(_project);
            _project.Logic.EditorMode = mode;
            _ = CaptureRecoveryAsync();
            ShowLogicEditor();
        };
        modes.Children.Add(button);
    }

    private void ShowStructuredTextEditor()
    {
        if (_project is null) return;
        PageContent.Children.Add(new TextBlock { Text = T("Structured Text", "Structured Text"), FontSize = 20, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        PageContent.Children.Add(new TextBlock
        {
            Text = T(
                "Поддерживается безопасное подмножество ST: :=, NOT, AND, OR, XOR, =, >, <, SET(), RESET(). Одна операция на присваивание; TRUE/FALSE и числа разрешены.",
                "Supported safe ST subset: :=, NOT, AND, OR, XOR, =, >, <, SET(), RESET(). One operation per assignment; TRUE/FALSE and numeric literals are supported."),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            Opacity = 0.7
        });
        var source = new TextBox
        {
            Text = string.IsNullOrWhiteSpace(_project.Logic.StructuredTextSource) ? _stCompiler.Render(_project) : _project.Logic.StructuredTextSource,
            AcceptsReturn = true,
            MinHeight = 300,
            TextWrapping = Avalonia.Media.TextWrapping.NoWrap,
            FontFamily = "Consolas"
        };
        PageContent.Children.Add(source);
        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var compile = new Button { Content = T("Компилировать ST", "Compile ST") };
        compile.Click += (_, _) =>
        {
            var result = _stCompiler.Compile(_project, source.Text ?? string.Empty);
            _stCompileMessage = result.Success
                ? T($"ST скомпилирован: {result.Statements} операторов.", $"ST compiled: {result.Statements} statements.")
                : string.Join(Environment.NewLine, result.Errors);
            if (result.Success) _ = CaptureRecoveryAsync();
            ShowLogicEditor();
        };
        var regenerate = new Button { Content = T("Сформировать ST из FBD/LD", "Generate ST from FBD/LD") };
        regenerate.Click += (_, _) => { _project.Logic.StructuredTextSource = _stCompiler.Render(_project); ShowLogicEditor(); };
        var validate = new Button { Content = T("Проверить", "Validate") };
        validate.Click += (_, _) => ShowLogicValidation();
        actions.Children.Add(compile); actions.Children.Add(regenerate); actions.Children.Add(validate); PageContent.Children.Add(actions);
        if (!string.IsNullOrWhiteSpace(_stCompileMessage))
            PageContent.Children.Add(new TextBlock { Text = _stCompileMessage, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        AddStExamples();
    }

    private void AddStExamples()
    {
        PageContent.Children.Add(new TextBlock { Text = T("Пример", "Example"), FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 8, 0, 0) });
        PageContent.Children.Add(new TextBlock
        {
            Text = "Motor := Start AND SafetyOk;\nAlarm := NOT SensorOk;\nReady := Temperature < 80;\nSET(Latched);\nRESET(Latched);",
            FontFamily = "Consolas",
            Opacity = 0.7
        });
    }

    private void AddVariables(LogicEditorView view)
    {
        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock { Text = T("Переменные", "Variables"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (view.Variables.Count == 0) PageContent.Children.Add(new TextBlock { Text = T("Переменных пока нет.", "No variables yet."), Opacity = 0.65 });
        else foreach (var variable in view.Variables) AddVariableCard(variable);
    }

    private void AddVariableCard(LogicVariable variable)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        row.Children.Add(new TextBlock { Text = variable.Name, VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(new TextBlock { Text = variable.DataType.ToString(), Opacity = 0.6, VerticalAlignment = VerticalAlignment.Center });
        var remove = new Button { Content = T("Удалить", "Remove") };
        remove.Click += (_, _) => { _logicProjectOperations.RemoveVariable(_project!.Logic, variable.Id); _ = CaptureRecoveryAsync(); ShowLogicEditor(); };
        row.Children.Add(remove); PageContent.Children.Add(row);
    }

    private void AddNetworkCard(LogicNetwork network)
    {
        var box = new Border { Padding = new Avalonia.Thickness(12), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
        var panel = new StackPanel { Spacing = 8 };
        panel.Children.Add(new TextBlock { Text = network.Name, FontSize = 17, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (network.Instructions.Count == 0)
            panel.Children.Add(new TextBlock { Text = T("В сети пока нет операций.", "No instructions in this network yet."), Opacity = 0.65 });
        else
            for (var index = 0; index < network.Instructions.Count; index++)
            {
                if (_project!.Logic.EditorMode == LogicEditorMode.Ladder) AddLadderRung(panel, network.Instructions[index], index);
                else AddFbdBlock(panel, network, network.Instructions[index], index);
            }
        box.Child = panel; PageContent.Children.Add(box);
    }

    private void AddFbdBlock(StackPanel parent, LogicNetwork network, LogicInstruction instruction, int index)
    {
        var visual = new Grid { ColumnDefinitions = new ColumnDefinitions("*,Auto,*"), Margin = new Avalonia.Thickness(0, 2) };
        var inputs = new TextBlock { Text = instruction.SourceB is null ? instruction.SourceA ?? "—" : $"{instruction.SourceA}\n{instruction.SourceB}", VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Avalonia.Thickness(0, 0, 10, 0) };
        var block = new Border { Padding = new Avalonia.Thickness(12, 8), Background = Avalonia.Media.Brush.Parse("#263456"), CornerRadius = new Avalonia.CornerRadius(5), Child = new TextBlock { Text = instruction.Kind.ToString(), FontWeight = Avalonia.Media.FontWeight.SemiBold } };
        var output = new TextBlock { Text = instruction.Target ?? "—", VerticalAlignment = VerticalAlignment.Center, Margin = new Avalonia.Thickness(10, 0, 0, 0) };
        Grid.SetColumn(inputs, 0); Grid.SetColumn(block, 1); Grid.SetColumn(output, 2); visual.Children.Add(inputs); visual.Children.Add(block); visual.Children.Add(output);
        parent.Children.Add(visual);
        AddInstructionEditor(parent, network, instruction, index);
    }

    private void AddLadderRung(StackPanel parent, LogicInstruction instruction, int index)
    {
        var rung = instruction.Kind switch
        {
            LogicInstructionKind.Not => $"|--[/ {instruction.SourceA} ]----------------( {instruction.Target} )--|",
            LogicInstructionKind.And => $"|--[ {instruction.SourceA} ]--[ {instruction.SourceB} ]----( {instruction.Target} )--|",
            LogicInstructionKind.Or => $"|--[ {instruction.SourceA} ]--+-------------( {instruction.Target} )--|\n|                 +--[ {instruction.SourceB} ]--+",
            LogicInstructionKind.Set => $"|----------------------------------(S {instruction.Target} )--|",
            LogicInstructionKind.Reset => $"|----------------------------------(R {instruction.Target} )--|",
            _ => $"|--[ {instruction.SourceA ?? instruction.Kind.ToString()} ]--------( {instruction.Target} )--|"
        };
        parent.Children.Add(new TextBlock { Text = $"{index + 1}. {rung}", FontFamily = "Consolas", TextWrapping = Avalonia.Media.TextWrapping.Wrap, Margin = new Avalonia.Thickness(0, 3) });
    }

    private void AddInstructionEditor(StackPanel parent, LogicNetwork network, LogicInstruction instruction, int index)
    {
        var border = new Border { Padding = new Avalonia.Thickness(8), Background = Avalonia.Media.Brush.Parse("#141D36"), CornerRadius = new Avalonia.CornerRadius(6) };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("34,120,*,*,*"), RowDefinitions = new RowDefinitions("Auto,Auto") };
        grid.Children.Add(new TextBlock { Text = $"{index + 1}.", VerticalAlignment = VerticalAlignment.Center });
        var kind = new TextBlock { Text = instruction.Kind.ToString(), VerticalAlignment = VerticalAlignment.Center, FontWeight = Avalonia.Media.FontWeight.SemiBold }; Grid.SetColumn(kind, 1); grid.Children.Add(kind);
        var target = new TextBox { Text = instruction.Target ?? string.Empty, Watermark = T("Цель", "Target") }; target.LostFocus += (_, _) => { _logicWorkflow.ConfigureInstruction(instruction, target.Text, instruction.SourceA, instruction.SourceB, instruction.Comment); _ = CaptureRecoveryAsync(); }; Grid.SetColumn(target, 2); grid.Children.Add(target);
        var sourceA = new TextBox { Text = instruction.SourceA ?? string.Empty, Watermark = T("Источник A", "Source A") }; sourceA.LostFocus += (_, _) => { _logicWorkflow.ConfigureInstruction(instruction, instruction.Target, sourceA.Text, instruction.SourceB, instruction.Comment); _ = CaptureRecoveryAsync(); }; Grid.SetColumn(sourceA, 3); grid.Children.Add(sourceA);
        var sourceB = new TextBox { Text = instruction.SourceB ?? string.Empty, Watermark = T("Источник B", "Source B") }; sourceB.LostFocus += (_, _) => { _logicWorkflow.ConfigureInstruction(instruction, instruction.Target, instruction.SourceA, sourceB.Text, instruction.Comment); _ = CaptureRecoveryAsync(); }; Grid.SetColumn(sourceB, 4); grid.Children.Add(sourceB);
        var comment = new TextBox { Text = instruction.Comment ?? string.Empty, Watermark = T("Комментарий", "Comment") }; Grid.SetRow(comment, 1); Grid.SetColumn(comment, 2); Grid.SetColumnSpan(comment, 3); comment.LostFocus += (_, _) => { _logicWorkflow.ConfigureInstruction(instruction, instruction.Target, instruction.SourceA, instruction.SourceB, comment.Text); _ = CaptureRecoveryAsync(); }; grid.Children.Add(comment);
        border.Child = grid; parent.Children.Add(border);
    }

    private void AddLogicBlockToSelectedOrFirst(LogicInstructionKind kind)
    {
        if (_project is null) return;
        var network = _project.Logic.Networks.FirstOrDefault() ?? _logicWorkflow.CreateNetwork(_project, T("Основная сеть", "Main network"));
        _logicWorkflow.AddInstruction(network, kind); _ = CaptureRecoveryAsync(); ShowLogicEditor();
    }

    private void AddSymbolList(LogicEditorView view)
    {
        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 12) });
        PageContent.Children.Add(new TextBlock { Text = T("Символы текущего проекта", "Current project symbols"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        foreach (var group in view.Symbols.GroupBy(x => x.Category))
        {
            PageContent.Children.Add(new TextBlock { Text = group.Key, FontWeight = Avalonia.Media.FontWeight.SemiBold, Opacity = 0.75, Margin = new Avalonia.Thickness(0, 5, 0, 0) });
            foreach (var symbol in group) PageContent.Children.Add(new TextBlock { Text = $"• {symbol.Name} ({symbol.DataType})" });
        }
    }

    private void ShowLogicValidation()
    {
        if (_project is null) return;
        SetHeader(T("Проверка логики", "Logic validation"), T("Все ошибки показываются до выполнения симуляции.", "All errors are shown before simulation execution."));
        PageContent.Children.Clear();
        var messages = _logicWorkflow.Validate(_project);
        if (messages.Count == 0) PageContent.Children.Add(new TextBlock { Text = "✓ " + T("Ошибок не найдено.", "No errors found.") });
        else foreach (var message in messages)
        {
            var symbol = message.Severity switch { LogicValidationSeverity.Error => "✕", LogicValidationSeverity.Warning => "⚠", _ => "ⓘ" };
            PageContent.Children.Add(new TextBlock { Text = $"{symbol} [{message.Severity}] {message.Message}", TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        }
        var back = new Button { Content = T("Вернуться к редактору", "Back to editor"), Margin = new Avalonia.Thickness(0, 10, 0, 0) };
        back.Click += (_, _) => ShowLogicEditor(); PageContent.Children.Add(back);
    }
}
