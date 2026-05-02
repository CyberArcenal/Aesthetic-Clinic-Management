using AestheticClinicAPI.Data;
using AestheticClinicAPI.Modules.Dashboard.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AestheticClinicAPI.Modules.Dashboard.Services;

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync()
    {
        var now = DateTime.UtcNow;
        var startThisMonth = new DateTime(now.Year, now.Month, 1);
        var startLastMonth = startThisMonth.AddMonths(-1);
        var endLastMonth = startThisMonth.AddDays(-1);
        var start30DaysAgo = now.AddDays(-30);

        // ----- Revenue from Payments -----
        var revenueThisMonth = await _db.Payments
            .Where(p => p.PaymentDate >= startThisMonth)
            .SumAsync(p => p.Amount);

        var revenueLastMonth = await _db.Payments
            .Where(p => p.PaymentDate >= startLastMonth && p.PaymentDate <= endLastMonth)
            .SumAsync(p => p.Amount);

        // ----- Appointments count -----
        var appsThisMonth = await _db.Appointments
            .CountAsync(a => a.AppointmentDateTime >= startThisMonth);
        var appsLastMonth = await _db.Appointments
            .CountAsync(a => a.AppointmentDateTime >= startLastMonth && a.AppointmentDateTime <= endLastMonth);

        // ----- New clients -----
        var newClientsThisMonth = await _db.Clients
            .CountAsync(c => c.CreatedAt >= startThisMonth);
        var newClientsLastMonth = await _db.Clients
            .CountAsync(c => c.CreatedAt >= startLastMonth && c.CreatedAt <= endLastMonth);

        // ----- Average ticket (revenue / completed appointments this month) -----
        var completedAppsThisMonth = await _db.Appointments
            .CountAsync(a => a.Status == "Completed" && a.AppointmentDateTime >= startThisMonth);
        var avgTicket = completedAppsThisMonth > 0 ? revenueThisMonth / completedAppsThisMonth : 0;

        // ----- Daily Revenue (last 7 days) -----
        var last7Days = Enumerable.Range(0, 7)
            .Select(i => now.Date.AddDays(-i))
            .OrderBy(d => d)
            .ToList();
        var dailyRevenue = new List<DailyRevenueDto>();
        foreach (var day in last7Days)
        {
            var nextDay = day.AddDays(1);
            var dayRevenue = await _db.Payments
                .Where(p => p.PaymentDate >= day && p.PaymentDate < nextDay)
                .SumAsync(p => p.Amount);
            var dayApps = await _db.Appointments
                .CountAsync(a => a.AppointmentDateTime >= day && a.AppointmentDateTime < nextDay);
            dailyRevenue.Add(new DailyRevenueDto
            {
                Date = day,
                Revenue = dayRevenue,
                Appointments = dayApps
            });
        }

        // ----- Top Services (by revenue, using completed appointments) -----
        var serviceStats = await _db.Appointments
            .Where(a => a.Status == "Completed" && a.Treatment != null)
            .GroupBy(a => new { a.Treatment!.Id, a.Treatment.Name, a.Treatment.Category })
            .Select(g => new
            {
                g.Key.Name,
                g.Key.Category,
                Count = g.Count(),
                Revenue = g.Sum(a => a.Treatment!.Price)
            })
            .ToListAsync();

        var totalRevenueAll = serviceStats.Sum(s => s.Revenue);
        var topServices = serviceStats
            .OrderByDescending(s => s.Revenue)
            .Take(5)
            .Select(s => new TopServicePerformanceDto
            {
                ServiceName = s.Name,
                Category = s.Category ?? "Uncategorized",
                AppointmentCount = s.Count,
                Revenue = s.Revenue,
                PercentageOfTotal = totalRevenueAll > 0 ? (double)(s.Revenue / totalRevenueAll * 100) : 0
            }).ToList();

        // ----- Appointment Funnel (current month) -----
        var funnel = await _db.Appointments
            .Where(a => a.AppointmentDateTime >= startThisMonth)
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(k => k.Status, v => v.Count);
        int scheduled = funnel.GetValueOrDefault("Scheduled", 0);
        int confirmed = funnel.GetValueOrDefault("Confirmed", 0);
        int completed = funnel.GetValueOrDefault("Completed", 0);
        int cancelled = funnel.GetValueOrDefault("Cancelled", 0);
        int noShow = funnel.GetValueOrDefault("NoShow", 0);
        int totalBooked = scheduled + confirmed;
        double completionRate = totalBooked > 0 ? (double)completed / totalBooked * 100 : 0;
        double cancelRate = totalBooked > 0 ? (double)cancelled / totalBooked * 100 : 0;
        double noShowRate = totalBooked > 0 ? (double)noShow / totalBooked * 100 : 0;

        // ----- Client Retention -----
        var totalClients = await _db.Clients.CountAsync();
        var newClients30Days = await _db.Clients
            .CountAsync(c => c.CreatedAt >= start30DaysAgo);
        var returningClients = await _db.Appointments
            .Where(a => a.AppointmentDateTime >= start30DaysAgo && a.ClientId != 0)
            .GroupBy(a => a.ClientId)
            .CountAsync(g => g.Count() >= 2);
        double retentionRate = totalClients > 0 ? (double)returningClients / totalClients * 100 : 0;

        // ----- Staff Performance (Top 3 by revenue) -----
        var staffStats = await _db.Appointments
            .Where(a => a.Status == "Completed" && a.StaffId != null && a.Treatment != null)
            .GroupBy(a => new { a.StaffId, StaffName = a.AssignedStaff ?? "Unknown" })
            .Select(g => new
            {
                g.Key.StaffName,
                Completed = g.Count(),
                Revenue = g.Sum(a => a.Treatment!.Price)
            })
            .OrderByDescending(s => s.Revenue)
            .Take(3)
            .ToListAsync();
        var staffPerformance = staffStats.Select(s => new StaffPerformanceDto
        {
            StaffName = s.StaffName,
            CompletedAppointments = s.Completed,
            RevenueGenerated = s.Revenue,
            UtilizationRate = 0 // placeholder: need working hours
        }).ToList();

        // ----- Forecast (simple projection based on last 7 days) -----
        var last7DaysRevenue = dailyRevenue.Sum(d => d.Revenue);
        var last7DaysApps = dailyRevenue.Sum(d => d.Appointments);
        var forecast = new ForecastDto
        {
            ProjectedRevenueNextWeek = last7DaysRevenue,
            ProjectedAppointmentsNextWeek = last7DaysApps,
            Note = "Based on last 7 days (simple projection)."
        };

        // ----- Assemble result -----
        return new DashboardStatsDto
        {
            Kpis = new KpiCardsDto
            {
                RevenueThisMonth = revenueThisMonth,
                RevenueLastMonth = revenueLastMonth,
                RevenueChangePercent = revenueLastMonth > 0 ? (double)((revenueThisMonth - revenueLastMonth) / revenueLastMonth * 100) : 0,
                AppointmentsThisMonth = appsThisMonth,
                AppointmentsLastMonth = appsLastMonth,
                AppointmentsChangePercent = appsLastMonth > 0 ? (double)((appsThisMonth - appsLastMonth) / appsLastMonth * 100) : 0,
                NewClientsThisMonth = newClientsThisMonth,
                NewClientsLastMonth = newClientsLastMonth,
                NewClientsChangePercent = newClientsLastMonth > 0 ? (double)((newClientsThisMonth - newClientsLastMonth) / newClientsLastMonth * 100) : 0,
                AverageTicket = avgTicket
            },
            DailyRevenue = dailyRevenue,
            TopServices = topServices,
            AppointmentFunnel = new AppointmentFunnelDto
            {
                Scheduled = scheduled,
                Confirmed = confirmed,
                Completed = completed,
                Cancelled = cancelled,
                NoShow = noShow,
                CompletionRate = completionRate,
                CancellationRate = cancelRate,
                NoShowRate = noShowRate
            },
            ClientRetention = new ClientRetentionDto
            {
                TotalClients = totalClients,
                NewClients30Days = newClients30Days,
                ReturningClients30Days = returningClients,
                RetentionRate = retentionRate
            },
            StaffPerformance = staffPerformance,
            Forecast = forecast
        };
    }
}