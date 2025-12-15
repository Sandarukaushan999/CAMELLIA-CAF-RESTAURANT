namespace CamelliaPOS.WPF.Models;

public class DailyAnalytics
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal WasteCost { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<HourlySale> HourlySales { get; set; } = new();
    public List<ItemStat> TopItems { get; set; } = new();
    public List<ItemStat> LeastItems { get; set; } = new();
}

public class HourlySale
{
    public int Hour { get; set; }
    public decimal Revenue { get; set; }
    public int Count { get; set; }
}

public class ItemStat
{
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Revenue { get; set; }
}

public class WeeklyAnalytics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailySale> DailySales { get; set; } = new();
}

public class MonthlyAnalytics
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<DailySale> DailySales { get; set; } = new();
}

public class YearlyAnalytics
{
    public int Year { get; set; }
    public decimal TotalRevenue { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<MonthlySale> MonthlySales { get; set; } = new();
}

public class DailySale
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int Count { get; set; }
}

public class MonthlySale
{
    public int Month { get; set; }
    public decimal Revenue { get; set; }
    public int Count { get; set; }
}

public class ProfitAnalytics
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Revenue { get; set; }
    public decimal Cost { get; set; }
    public decimal WasteCost { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
}

public class BestSellersResponse
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<ItemStat> TopItems { get; set; } = new();
}

