namespace DashboardTUI.Models;

public record struct SystemMetrics(
    double CpuPercent,
    double RamUsedGb,
    double RamTotalGb,
    string SystemUptime
);
