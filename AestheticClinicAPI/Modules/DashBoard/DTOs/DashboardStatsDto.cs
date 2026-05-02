namespace AestheticClinicAPI.Modules.Dashboard.DTOs;

public class DashboardStatsDto
{
    public KpiCardsDto Kpis { get; set; } = new();
    public List<DailyRevenueDto> DailyRevenue { get; set; } = new();
    public List<TopServicePerformanceDto> TopServices { get; set; } = new();
    public AppointmentFunnelDto AppointmentFunnel { get; set; } = new();
    public ClientRetentionDto ClientRetention { get; set; } = new();
    public List<StaffPerformanceDto> StaffPerformance { get; set; } = new();
    public ForecastDto Forecast { get; set; } = new();
}

public class KpiCardsDto
{
    public decimal RevenueThisMonth { get; set; }
    public decimal RevenueLastMonth { get; set; }
    public double RevenueChangePercent { get; set; }
    public int AppointmentsThisMonth { get; set; }
    public int AppointmentsLastMonth { get; set; }
    public double AppointmentsChangePercent { get; set; }
    public int NewClientsThisMonth { get; set; }
    public int NewClientsLastMonth { get; set; }
    public double NewClientsChangePercent { get; set; }
    public decimal AverageTicket { get; set; }
}

public class DailyRevenueDto
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Appointments { get; set; }
}

public class TopServicePerformanceDto
{
    public string ServiceName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int AppointmentCount { get; set; }
    public decimal Revenue { get; set; }
    public double PercentageOfTotal { get; set; }
}

public class AppointmentFunnelDto
{
    public int Scheduled { get; set; }
    public int Confirmed { get; set; }
    public int Completed { get; set; }
    public int Cancelled { get; set; }
    public int NoShow { get; set; }
    public double CompletionRate { get; set; }
    public double CancellationRate { get; set; }
    public double NoShowRate { get; set; }
}

public class ClientRetentionDto
{
    public int TotalClients { get; set; }
    public int NewClients30Days { get; set; }
    public int ReturningClients30Days { get; set; }
    public double RetentionRate { get; set; }
}

public class StaffPerformanceDto
{
    public string StaffName { get; set; } = string.Empty;
    public int CompletedAppointments { get; set; }
    public decimal RevenueGenerated { get; set; }
    public double UtilizationRate { get; set; }
}

public class ForecastDto
{
    public decimal ProjectedRevenueNextWeek { get; set; }
    public int ProjectedAppointmentsNextWeek { get; set; }
    public string Note { get; set; } = string.Empty;
}