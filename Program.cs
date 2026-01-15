using DashboardTUI.Services;
using DashboardTUI.UI;
using Spectre.Console;

var collector = new MetricsCollector();

await AnsiConsole.Live(new Panel("Загрузка..."))
    .StartAsync(async ctx =>
    {
        bool running = true;
        while (running)
        {
            var metrics = collector.FetchCurrentMetrics();

            var table = ConsoleInterface.BuildDashboard(metrics);
            
            table.Caption = new TableTitle("[grey]Нажмите [white][bold]Q[/][/] для выхода[/]");

            ctx.UpdateTarget(table);

            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;

                if (key == ConsoleKey.Q || key == ConsoleKey.Escape)
                {
                    running = false;
                }
            }

            await Task.Delay(1000);
        }
    });

AnsiConsole.MarkupLine("[yellow]Программа завершена.[/]");