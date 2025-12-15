using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CamelliaPOS.API.Data;
using CamelliaPOS.API.Models;
using System.Text;

namespace CamelliaPOS.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Manager")]
public class AnalyticsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AnalyticsController(ApplicationDbContext context)
    {
        _context = context;
    }

        [HttpGet("bestsellers")]
        public async Task<IActionResult> GetBestSellers([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate, [FromQuery] int top = 10)
        {
            var start = startDate ?? DateTime.UtcNow.AddDays(-7);
            var end = endDate ?? DateTime.UtcNow;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.OrderDate >= start && o.OrderDate <= end)
                .ToListAsync();

            var topItems = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => oi.MenuItem.Name)
                .Select(g => new
                {
                    ItemName = g.Key,
                    Quantity = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.TotalPrice)
                })
                .OrderByDescending(x => x.Quantity)
                .Take(top)
                .ToList();

            return Ok(new
            {
                StartDate = start,
                EndDate = end,
                TopItems = topItems
            });
        }

    [HttpGet("daily")]
    public async Task<IActionResult> GetDailySales([FromQuery] DateTime date)
    {
        var startDate = date.Date;
        var endDate = startDate.AddDays(1);

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
            .ToListAsync();

        var hourlySales = Enumerable.Range(0, 24)
            .Select(hour => new
            {
                Hour = hour,
                Revenue = orders.Where(o => o.OrderDate.Hour == hour).Sum(o => o.Total),
                Count = orders.Count(o => o.OrderDate.Hour == hour)
            })
            .ToList();

        var topItems = orders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => oi.MenuItem.Name)
            .Select(g => new
            {
                ItemName = g.Key,
                Quantity = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(10)
            .ToList();

        var leastItems = orders
            .SelectMany(o => o.OrderItems)
            .GroupBy(oi => oi.MenuItem.Name)
            .Select(g => new
            {
                ItemName = g.Key,
                Quantity = g.Sum(oi => oi.Quantity),
                Revenue = g.Sum(oi => oi.TotalPrice)
            })
            .OrderBy(x => x.Quantity)
            .Take(10)
            .ToList();

        var totalRevenue = orders.Sum(o => o.Total);
        var totalCost = orders
            .SelectMany(o => o.OrderItems)
            .Sum(oi => oi.MenuItem.Cost * oi.Quantity);
        var totalProfit = totalRevenue - totalCost;
        var averageOrderValue = orders.Count > 0 ? totalRevenue / orders.Count : 0;

        var wasteCost = await _context.Wastes
            .Where(w => w.WasteDate >= startDate && w.WasteDate < endDate)
            .SumAsync(w => w.Cost);

        return Ok(new
        {
            Date = date.Date,
            TotalRevenue = totalRevenue,
            TotalCost = totalCost,
            TotalProfit = totalProfit - wasteCost,
            WasteCost = wasteCost,
            OrderCount = orders.Count,
            AverageOrderValue = averageOrderValue,
            HourlySales = hourlySales,
            TopItems = topItems,
            LeastItems = leastItems
        });
    }

    [HttpGet("weekly")]
    public async Task<IActionResult> GetWeeklySales([FromQuery] DateTime startDate)
    {
        var endDate = startDate.AddDays(7);

        var orders = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
            .ToListAsync();

        var dailySales = Enumerable.Range(0, 7)
            .Select(day => new
            {
                Date = startDate.AddDays(day).Date,
                Revenue = orders.Where(o => o.OrderDate.Date == startDate.AddDays(day).Date).Sum(o => o.Total),
                Count = orders.Count(o => o.OrderDate.Date == startDate.AddDays(day).Date)
            })
            .ToList();

        return Ok(new
        {
            StartDate = startDate.Date,
            EndDate = endDate.Date,
            TotalRevenue = orders.Sum(o => o.Total),
            OrderCount = orders.Count,
            AverageOrderValue = orders.Count > 0 ? orders.Sum(o => o.Total) / orders.Count : 0,
            DailySales = dailySales
        });
    }

    [HttpGet("monthly")]
    public async Task<IActionResult> GetMonthlySales([FromQuery] int year, [FromQuery] int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var orders = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
            .ToListAsync();

        var dailySales = Enumerable.Range(1, DateTime.DaysInMonth(year, month))
            .Select(day => new
            {
                Date = new DateTime(year, month, day),
                Revenue = orders.Where(o => o.OrderDate.Date == new DateTime(year, month, day)).Sum(o => o.Total),
                Count = orders.Count(o => o.OrderDate.Date == new DateTime(year, month, day))
            })
            .ToList();

        return Ok(new
        {
            Year = year,
            Month = month,
            TotalRevenue = orders.Sum(o => o.Total),
            OrderCount = orders.Count,
            AverageOrderValue = orders.Count > 0 ? orders.Sum(o => o.Total) / orders.Count : 0,
            DailySales = dailySales
        });
    }

    [HttpGet("yearly")]
    public async Task<IActionResult> GetYearlySales([FromQuery] int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = startDate.AddYears(1);

        var orders = await _context.Orders
            .Where(o => o.OrderDate >= startDate && o.OrderDate < endDate)
            .ToListAsync();

        var monthlySales = Enumerable.Range(1, 12)
            .Select(m => new
            {
                Month = m,
                Revenue = orders.Where(o => o.OrderDate.Month == m).Sum(o => o.Total),
                Count = orders.Count(o => o.OrderDate.Month == m)
            })
            .ToList();

        return Ok(new
        {
            Year = year,
            TotalRevenue = orders.Sum(o => o.Total),
            OrderCount = orders.Count,
            AverageOrderValue = orders.Count > 0 ? orders.Sum(o => o.Total) / orders.Count : 0,
            MonthlySales = monthlySales
        });
    }

    [HttpGet("profit")]
    public async Task<IActionResult> GetProfitAnalysis([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
            .ToListAsync();

        var revenue = orders.Sum(o => o.Total);
        var cost = orders
            .SelectMany(o => o.OrderItems)
            .Sum(oi => oi.MenuItem.Cost * oi.Quantity);

        var wasteCost = await _context.Wastes
            .Where(w => w.WasteDate >= start && w.WasteDate <= end)
            .SumAsync(w => w.Cost);

        var profit = revenue - cost - wasteCost;
        var profitMargin = revenue > 0 ? (profit / revenue) * 100 : 0;

        return Ok(new
        {
            StartDate = start,
            EndDate = end,
            Revenue = revenue,
            Cost = cost,
            WasteCost = wasteCost,
            Profit = profit,
            ProfitMargin = profitMargin
        });
    }

    [HttpGet("export/csv")]
    public async Task<IActionResult> ExportCsv([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;

        var orders = await _context.Orders
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.OrderDate >= start && o.OrderDate <= end)
            .OrderBy(o => o.OrderDate)
            .ToListAsync();

        var lines = new List<string>
        {
            "OrderNumber,OrderDate,Item,Quantity,UnitPrice,Total,PaymentMethod"
        };

        foreach (var o in orders)
        {
            foreach (var item in o.OrderItems)
            {
                lines.Add($"{o.OrderNumber},{o.OrderDate:O},{item.MenuItem.Name},{item.Quantity},{item.UnitPrice},{item.TotalPrice},{o.PaymentMethod}");
            }
        }

        var bytes = System.Text.Encoding.UTF8.GetBytes(string.Join(Environment.NewLine, lines));
        var fileName = $"orders_{start:yyyyMMdd}_{end:yyyyMMdd}.csv";
        return File(bytes, "text/csv", fileName);
    }
}

