using System.Globalization;
using DashboardTUI.Models;

namespace DashboardTUI.Services;

public class MetricsCollector
{
    
    private long _lastIdle;
    private long _lastTotal;
    
    private const string MemInfoPath = "/proc/meminfo";
    private const string UptimeInfoPath = "/proc/uptime";
    private const string StatInfoPath = "/proc/stat";
    
    public SystemMetrics FetchCurrentMetrics()
    {
        var lines = File.Exists(MemInfoPath) 
            ? File.ReadAllLines(MemInfoPath) 
            : Array.Empty<string>();
        double totalKb = GetValue(lines, "MemTotal");
        double availableKb = GetValue(lines, "MemAvailable");
        
        if (availableKb == 0) availableKb = GetValue(lines, "MemFree");

        double totalGb = totalKb / 1048576;
        double usedGb = (totalKb - availableKb) / 1048576;
        
        double cpu = GetCpuUsage();
        
        return new SystemMetrics(
            CpuPercent:cpu,
            RamUsedGb:Math.Round(usedGb,2),
            RamTotalGb: Math.Round(totalGb,2),
            SystemUptime:GetUptime());
    }

    private double GetValue(string[] lines, string key)
    {
        var line = lines.FirstOrDefault(l=>l.StartsWith(key, StringComparison.OrdinalIgnoreCase));
        if (string.IsNullOrEmpty(line)) return 0;
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1 && double.TryParse(parts[1], CultureInfo.InvariantCulture, out double value))
        {
            return value;
        }

        return 0;
    }

    private string GetUptime()
    {
        try
        {
            var uptimeContent = File.ReadAllText(UptimeInfoPath).Split(' ')[0];
            var seconds = double.Parse(uptimeContent, CultureInfo.InvariantCulture);
            var time = TimeSpan.FromSeconds(seconds);
            return $"{time.Days}d {time.Hours}h {time.Minutes}m";
        }
        catch
        {
            return "0d 0h 0m";
        }
    }

    private (long Idle, long Total) GetCpuTicks()
    {
        var line = File.ReadLines(StatInfoPath).First();
        var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        long user = long.Parse(parts[1]);
        long nice = long.Parse(parts[2]);
        long system = long.Parse(parts[3]);
        long idle = long.Parse(parts[4]);
        long iowait = long.Parse(parts[5]);
        long irq = long.Parse(parts[6]);
        long softirq = long.Parse(parts[7]);
        long steal = long.Parse(parts[8]);

        long total = user + nice + system + idle + iowait + irq + softirq + steal;

        return (idle, total);
    }

    private double GetCpuUsage()
    {
        var (currentIdle, currentTotal) = GetCpuTicks();

        if (_lastTotal == 0)
        {
            _lastTotal = currentTotal;
            _lastIdle = currentIdle;
            return 0;
        }

        double totalDelta = currentTotal - _lastTotal;
        double idleDelta = currentIdle - _lastIdle;
        
        _lastTotal = currentTotal;
        _lastIdle = currentIdle;

        if (totalDelta <= 0) return 0;

        double usagePercent = (1.0 - (idleDelta / totalDelta)) * 100.0;
        
        return Math.Clamp(Math.Round(usagePercent, 1), 0.0, 100.0);
    }
}