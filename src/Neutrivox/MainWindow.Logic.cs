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

    private void LogicNavigationButton_OnClick(object? sender, RoutedEventArgs e) => ShowLogicEditor();

    private void ShowLogicEditor()
    {
        SetHeader(
            T("Логика контроллера", "Controller logic"),
            T("Создавайте сети, добавляйте операции и используйте реальные символы текущего проекта без отдельного проекта для симуляции.",
              "Create networks, add operations and use real symbols from the current project without a separate simulation project."));
        PageContent.Children.Clear();

        if (_project is null)
        {
            AddAction(T("Создать проект", "Create project"),
                T("Редактор логики работает внутри единого проекта Neutrivox.", "The logic editor works inside the unified Neutrivox project."),
                (_, _) => NewProjectButton_OnClick(null, null));
            return;
        }

        var view = _logicPresenter.Build(_project);
        var summary = _projectSummary.CreateHumanReadableSummary(_project);

        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8, Margin = new Avalonia.Thickness(0, 0, 0, 12) };
        var newNetwork = new Button { Content = T("+ Сеть", "+ Network") };
        newNetwork.Click += (_, _) => { _logicWorkflow.CreateNetwork(_project, T("Новая сеть", "New network")); ShowLogicEditor(); };
        var newVariable = new Button { Content = T("+ Переменная", "+ Variable") };
        newVariable.Click += (_, _) => { _logicWorkflow.CreateVariable(_project, T("Переменная", "Variable"), TagDataType.Boolean); ShowLogicEditor(); };
        var validate = new Button { Content = T("Проверить", "Validate") };
        validate.Click += (_, _) => ShowLogicValidation();
        toolbar.Children.Add(newNetwork);
        toolbar.Children.Add(newVariable);
        toolbar.Children.Add(validate);
        PageContent.Children.Add(toolbar);

        PageContent.Children.Add(new TextBlock { Text = T("Инструменты логики", "Logic toolbox"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        foreach (var block in view.Toolbox)
        {
            var add = new Button { Content = $"{block.DisplayName} — {block.Description}", HorizontalContentAlignment = HorizontalAlignment.Left, Margin = new Avalonia.Thickness(0, 2) };
            add.Click += (_, _) => AddLogicBlockToSelectedOrFirst(block.Kind);
            PageContent.Children.Add(add);
        }

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock { Text = T("Переменные", "Variables"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (view.Variables.Count == 0)
            PageContent.Children.Add(new TextBlock { Text = T("Переменных пока нет.", "No variables yet."), Opacity = 0.65 });
        else
            foreach (var variable in view.Variables)
                AddVariableCard(variable);

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 10) });
        PageContent.Children.Add(new TextBlock { Text = T("Логические сети", "Logic networks"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        if (view.Networks.Count == 0)
            PageContent.Children.Add(new TextBlock { Text = T("Создайте первую сеть кнопкой «+ Сеть».", "Create the first network with '+ Network'."), Opacity = 0.65 });
        else
            foreach (var network in view.Networks)
                AddNetworkCard(network);

        PageContent.Children.Add(new Separator { Margin = new Avalonia.Thickness(0, 12) });
        var symbolsHeader = new TextBlock { Text = T("Символы текущего проекта", "Current project symbols"), FontSize = 18, FontWeight = Avalonia.Media.FontWeight.SemiBold };
        PageContent.Children.Add(symbolsHeader);
        foreach (var group in view.Symbols.GroupBy(x => x.Category))
        {
            PageContent.Children.Add(new TextBlock { Text = group.Key, FontWeight = Avalonia.Media.FontWeight.SemiBold, Opacity = 0.75, Margin = new Avalonia.Thickness(0, 5, 0, 0) });
            foreach (var symbol in group)
                PageContent.Children.Add(new TextBlock { Text = $"• {symbol.Name} ({symbol.DataType})" });
        }

        var status = new Border { Padding = new Avalonia.Thickness(12), Margin = new Avalonia.Thickness(0, 12, 0, 0), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
        var statusPanel = new StackPanel { Spacing = 4 };
        statusPanel.Children.Add(new TextBlock { Text = T("Готовность логики", "Logic readiness"), FontWeight = Avalonia.Media.FontWeight.SemiBold });
        statusPanel.Children.Add(new TextBlock { Text = view.Readiness.Success ? "✓ " + T("Готова к симуляции", "Ready for simulation") : "⚠ " + T("Найдены ошибки", "Errors detected") });
        statusPanel.Children.Add(new TextBlock { Text = summary, Opacity = 0.55, TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        status.Child = statusPanel;
        PageContent.Children.Add(status);
    }

    private void AddVariableCard(LogicVariable variable)
    {
        var row = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 10 };
        row.Children.Add(new TextBlock { Text = variable.Name, VerticalAlignment = VerticalAlignment.Center });
        row.Children.Add(new TextBlock { Text = variable.DataType.ToString(), Opacity = 0.6, VerticalAlignment = VerticalAlignment.Center });
        var remove = new Button { Content = T("Удалить", "Remove") };
        remove.Click += (_, _) => { _logicProjectOperations.RemoveVariable(_project!.Logic, variable.Id); ShowLogicEditor(); };
        row.Children.Add(remove);
        PageContent.Children.Add(row);
    }

    private void AddNetworkCard(LogicNetwork network)
    {
        var box = new Border { Padding = new Avalonia.Thickness(12), Background = Avalonia.Media.Brush.Parse("#0D152B"), CornerRadius = new Avalonia.CornerRadius(8) };
        var panel = new StackPanel { Spacing = 8 };
        var head = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        head.Children.Add(new TextBlock { Text = network.Name, FontSize = 17, FontWeight = Avalonia.Media.FontWeight.SemiBold });
        head.Children.Add(new TextBlock { Text = network.Enabled ? T("Включена", "Enabled") : T("Выключена", "Disabled"), Opacity = 0.6 });
        panel.Children.Add(head);

        if (network.Instructions.Count == 0)
            panel.Children.Add(new TextBlock { Text = T("В сети пока нет операций.", "No instructions in this network yet."), Opacity = 0.65 });
        else
            for (var index = 0; index < network.Instructions.Count; index++)
                AddInstructionEditor(panel, network, network.Instructions[index], index);

        var actions = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 8 };
        var copy = new Button { Content = T("Добавить COPY", "Add COPY") };
        copy.Click += (_, _) => { _logicWorkflow.AddInstruction(network, LogicInstructionKind.Copy); ShowLogicEditor(); };
        var and = new Button { Content = T("Добавить AND", "Add AND") };
        and.Click += (_, _) => { _logicWorkflow.AddInstruction(network, LogicInstructionKind.And); ShowLogicEditor(); };
        var not = new Button { Content = T("Добавить NOT", "Add NOT") };
        not.Click += (_, _) => { _logicWorkflow.AddInstruction(network, LogicInstructionKind.Not); ShowLogicEditor(); };
        actions.Children.Add(copy); actions.Children.Add(and); actions.Children.Add(not);
        panel.Children.Add(actions);
        box.Child = panel;
        PageContent.Children.Add(box);
    }

    private void AddInstructionEditor(StackPanel parent, LogicNetwork network, LogicInstruction instruction, int index)
    {
        var border = new Border { Padding = new Avalonia.Thickness(8), Background = Avalonia.Media.Brush.Parse("#141D36"), CornerRadius = new Avalonia.CornerRadius(6) };
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("34,120,*,*,*"), RowDefinitions = new RowDefinitions("Auto,Auto") };
        grid.Children.Add(new TextBlock { Text = $"{index + 1}.", VerticalAlignment = VerticalAlignment.Center });
        var kind = new TextBlock { Text = instruction.Kind.ToString(), VerticalAlignment = VerticalAlignment.Center, FontWeight = Avalonia.Media.FontWeight.SemiBold }; Grid.SetColumn(kind, 1); grid.Children.Add(kind);
        var target = new TextBox { Text = instruction.Target ?? string.Empty, Watermark = T("Цель", "Target") }; target.LostFocus += (_, _) => _logicWorkflow.ConfigureInstruction(instruction, target.Text, instruction.SourceA, instruction.SourceB, instruction.Comment); Grid.SetColumn(target, 2); grid.Children.Add(target);
        var sourceA = new TextBox { Text = instruction.SourceA ?? string.Empty, Watermark = T("Источник A", "Source A") }; sourceA.LostFocus += (_, _) => _logicWorkflow.ConfigureInstruction(instruction, instruction.Target, sourceA.Text, instruction.SourceB, instruction.Comment); Grid.SetColumn(sourceA, 3); grid.Children.Add(sourceA);
        var sourceB = new TextBox { Text = instruction.SourceB ?? string.Empty, Watermark = T("Источник B", "Source B") }; sourceB.LostFocus += (_, _) => _logicWorkflow.ConfigureInstruction(instruction, instruction.Target, instruction.SourceA, sourceB.Text, instruction.Comment); Grid.SetColumn(sourceB, 4); grid.Children.Add(sourceB);
        var comment = new TextBox { Text = instruction.Comment ?? string.Empty, Watermark = T("Комментарий", "Comment") }; Grid.SetRow(comment, 1); Grid.SetColumn(comment, 2); Grid.SetColumnSpan(comment, 3); comment.LostFocus += (_, _) => _logicWorkflow.ConfigureInstruction(instruction, instruction.Target, instruction.SourceA, instruction.SourceB, comment.Text); grid.Children.Add(comment);
        border.Child = grid;
        parent.Children.Add(border);
    }

    private void AddLogicBlockToSelectedOrFirst(LogicInstructionKind kind)
    {
        if (_project is null) return;
        var network = _project.Logic.Networks.FirstOrDefault();
        if (network is null) network = _logicWorkflow.CreateNetwork(_project, T("Основная сеть", "Main network"));
        _logicWorkflow.AddInstruction(network, kind);
        ShowLogicEditor();
    }

    private void ShowLogicValidation()
    {
        if (_project is null) return;
        SetHeader(T("Проверка логики", "Logic validation"), T("Все ошибки показываются до выполнения симуляции.", "All errors are shown before simulation execution."));
        PageContent.Children.Clear();
        var messages = _logicWorkflow.Validate(_project);
        if (messages.Count == 0)
        {
            PageContent.Children.Add(new TextBlock { Text = "✓ " + T("Ошибок не найдено.", "No errors found.") });
            return;
        }
        foreach (var message in messages)
        {
            var symbol = message.Severity switch
            {
                LogicValidationSeverity.Error => "✕",
                LogicValidationSeverity.Warning => "⚠",
                _ => "ⓘ"
            };
            PageContent.Children.Add(new TextBlock { Text = $"{symbol} [{message.Severity}] {message.Message}", TextWrapping = Avalonia.Media.TextWrapping.Wrap });
        }
        var back = new Button { Content = T("Вернуться к редактору", "Back to editor"), Margin = new Avalonia.Thickness(0, 10, 0, 0) };
        back.Click += (_, _) => ShowLogicEditor();
        PageContent.Children.Add(back);
    }
}
