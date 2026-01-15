using Spectre.Console;
using DashboardTUI.Models;

namespace DashboardTUI.UI;

public static class ConsoleInterface
{
    public static Table BuildDashboard(SystemMetrics metrics)
    {
        var table = new Table().Centered();
        table.AddColumn("[cyan]Параметр[/]");
        table.AddColumn("[cyan]Значение[/]");

        table.AddRow("Загрузка CPU", $"[yellow]{metrics.CpuPercent}%[/]");
        table.AddRow("Память (RAM)", $"[green]{metrics.RamUsedGb} / {metrics.RamTotalGb} GB[/]");
        table.AddRow("Uptime", $"[blue]{metrics.SystemUptime}[/]");

        table.Border(TableBorder.Rounded);
        table.Title = new TableTitle("[bold white]DASHBOARD TUI[/]");        
        return table;
    }
}