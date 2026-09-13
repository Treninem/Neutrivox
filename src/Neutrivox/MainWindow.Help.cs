using Avalonia.Controls;
using Neutrivox.Models;

namespace Neutrivox;

public partial class MainWindow
{
    private void HelpNavigationButton_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => ShowHelp();

    private void ShowHelp()
    {
        SetHeader(T("Помощь и руководство", "Help & guide"),
            T("Пошаговая работа с Neutrivox: от проекта до симуляции, подключения и безопасной передачи.",
              "Step-by-step Neutrivox workflow: project, simulation, connection and safe deployment."));
        PageContent.Children.Clear();

        AddHelpSection(T("1. Проект", "1. Project"), T(
            "Создайте новый проект или откройте файл .neutrivox. Кнопка «Сохранить» записывает текущий проект; автосохранение recovery создаётся примерно каждые 30 секунд и доступно на странице «Проекты».",
            "Create a new project or open a .neutrivox file. Save writes the current project; a recovery snapshot is created roughly every 30 seconds and can be restored from Projects."));

        AddHelpSection(T("2. Оборудование", "2. Equipment"), T(
            "Добавьте контроллер, программируемое реле, модуль I/O или шлюз. Точная модификация содержит подтверждённые каналы. Запись вида «семейство» нужна для проектирования и не выдумывает неизвестное количество входов/выходов.",
            "Add a PLC, programmable relay, I/O module or gateway. Exact variants contain verified channels. Family entries are for planning and intentionally do not invent unknown I/O counts."));

        AddHelpSection(T("3. I/O и схема", "3. I/O & diagram"), T(
            "В «Входы и выходы» подпишите назначение каналов. В «Схема» размещайте оборудование и создавайте связи. Это та же модель проекта — отдельного режима «без прибора» нет.",
            "Describe channel purposes in Inputs & outputs. Use Diagram to view equipment and connections. It is the same project model; there is no duplicate 'offline' project mode."));

        AddHelpSection(T("4. Логика FBD / Ladder / ST", "4. FBD / Ladder / ST logic"), T(
            "В «Логика» выберите FBD, Ladder/LD или Structured Text. Представления используют один общий IR. В ST поддерживаются :=, NOT, AND, OR, XOR, сравнения = > <, SET() и RESET(); затем нажмите «Компилировать ST».",
            "In Logic choose FBD, Ladder/LD or Structured Text. All representations use one common IR. ST supports :=, NOT, AND, OR, XOR, comparisons = > <, SET() and RESET(); then select Compile ST."));

        AddHelpSection(T("5. Симуляция", "5. Simulation"), T(
            "Запустите симуляцию без физического оборудования, изменяйте дискретные входы и выполняйте циклы логики. Просматривайте значения выходов и trace. Перед симуляцией исправьте ошибки логики.",
            "Run simulation without physical hardware, change digital inputs and execute logic cycles. Inspect outputs and trace. Fix logic validation errors before relying on the result."));

        AddHelpSection(T("6. Подключение", "6. Connection"), T(
            "Для RS-485 выберите COM-порт и запускайте ограниченный Modbus RTU discovery. Для Ethernet укажите конкретный IPv4-адрес или CIDR и сканируйте TCP/502. Найденный endpoint не считается известной моделью, пока профиль не подтверждён. Binding всегда подтверждается отдельно.",
            "For RS-485 select a COM port and run scoped Modbus RTU discovery. For Ethernet enter an IPv4 address or CIDR and scan TCP/502. A reachable endpoint is not treated as a known model until a profile is verified. Binding always requires separate confirmation."));

        AddHelpSection(T("7. Передача в прибор", "7. Deployment"), T(
            "Откройте «Подключение» и подготовьте план. Физическая запись разрешается только для точного аппаратно проверенного профиля и зарегистрированного адаптера. Перед запуском нужно пройти preflight и вручную ввести DEPLOY. Несколько целей выполняются строго последовательно.",
            "Open Connection and prepare a deployment plan. Physical writes are enabled only for an exact hardware-verified profile with a registered adapter. Preflight must pass and DEPLOY must be typed manually. Multiple targets run strictly sequentially."));

        AddHelpSection(T("8. ОВЕН ПР и ПМ210", "8. OWEN PR and PM210"), T(
            "ПМ210 — сетевой шлюз, а не ПЛК с универсальными I/O. Для поддерживаемой передачи программ в ПР Neutrivox использует интеграционную точку официальной Owen Logic Replication Utility и не подменяет её выдуманным Modbus-загрузчиком.",
            "PM210 is a network gateway, not a PLC with universal I/O. For supported PR program transfer Neutrivox integrates with the official Owen Logic Replication Utility and does not replace it with an invented Modbus programmer."));

        AddHelpSection(T("9. Лицензия и аккаунт", "9. License & account"), T(
            "Первый запуск даёт 7-дневный Professional trial. После него остаётся Free. Платный ключ подписан и привязан к устройству. Для работы на нескольких своих ПК создайте аккаунт с ключом покупки, затем входите на других ПК по аккаунту в пределах лимита тарифа. Пароль продавцу не передаётся.",
            "First launch starts a 7-day Professional trial, then falls back to Free. Paid keys are signed and device-bound. To use several PCs you own, create an account with the purchase key and then sign in on other PCs within the plan device limit. Never send your account password to the seller."));

        AddHelpSection(T("10. Проверка проекта", "10. Project validation"), T(
            "Перед сохранением финальной версии откройте «Проверка»: там объединены целостность проекта, диагностика логики и release pre-check. Красные ошибки являются блокирующими.",
            "Before treating a project as final, open Validation. It combines project integrity, logic diagnostics and a release pre-check. Red errors are blocking."));

        PageContent.Children.Add(new TextBlock
        {
            Text = T(
                "Безопасность: никогда не подключайте физическую запись к неизвестной модели и не используйте Neutrivox для оборудования, на изменение которого у вас нет полномочий.",
                "Safety: never enable physical writing for an unknown model and do not use Neutrivox on equipment you are not authorized to modify."),
            TextWrapping = Avalonia.Media.TextWrapping.Wrap,
            FontWeight = Avalonia.Media.FontWeight.SemiBold,
            Margin = new Avalonia.Thickness(0, 12, 0, 0)
        });
    }

    private void AddHelpSection(string title, string body)
    {
        PageContent.Children.Add(new TextBlock { Text = title, FontSize = 19, FontWeight = Avalonia.Media.FontWeight.SemiBold, Margin = new Avalonia.Thickness(0, 8, 0, 0) });
        PageContent.Children.Add(new TextBlock { Text = body, TextWrapping = Avalonia.Media.TextWrapping.Wrap, Opacity = 0.8 });
    }
}
