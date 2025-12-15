using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.IO;
using Newtonsoft.Json;
using CamelliaPOS.WPF.Models;
using Microsoft.Win32;

namespace CamelliaPOS.WPF.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;
    private string? _token;

    public ApiService()
    {
        _httpClient = new HttpClient
        {
            // Backend is listening on 5144 per console log; update if you run on another port.
            BaseAddress = new Uri("http://localhost:5144/api/")
        };
    }

    public void SetToken(string token)
    {
        _token = token;
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("auth/login", new { Username = username, Password = password });
        
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null)
            {
                SetToken(result.Token);
            }
            return result;
        }
        
        return null;
    }

    public async Task<List<Category>> GetCategoriesAsync()
    {
        var response = await _httpClient.GetAsync("categories");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<Category>>(json) ?? new List<Category>();
        }
        return new List<Category>();
    }

    public async Task<List<MenuItem>> GetMenuItemsByCategoryAsync(int categoryId)
    {
        var response = await _httpClient.GetAsync($"menuitems/category/{categoryId}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<List<MenuItem>>(json) ?? new List<MenuItem>();
        }
        return new List<MenuItem>();
    }

    public async Task<List<MenuItem>> GetAllMenuItemsAsync()
    {
        var response = await _httpClient.GetAsync("menuitems/all");
        if (!response.IsSuccessStatusCode) return new List<MenuItem>();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<MenuItem>>(json) ?? new List<MenuItem>();
    }

    public async Task<bool> SetMenuItemActiveAsync(int id, bool isActive)
    {
        var response = await _httpClient.PostAsJsonAsync($"menuitems/{id}/set-active", new { IsActive = isActive });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateCategoryAsync(string name, string icon, string color, int displayOrder)
    {
        var response = await _httpClient.PostAsJsonAsync("categories", new
        {
            Name = name,
            Icon = icon,
            Color = color,
            DisplayOrder = displayOrder
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateMenuItemAsync(MenuItemCreateRequest req)
    {
        var response = await _httpClient.PostAsJsonAsync("menuitems", req);
        return response.IsSuccessStatusCode;
    }

    public async Task<MenuItem?> GetMenuItemByBarcodeAsync(string barcode)
    {
        var response = await _httpClient.GetAsync($"menuitems/barcode/{barcode}");
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<MenuItem>(json);
        }
        return null;
    }

    public async Task<(OrderResponse? order, string? error)> CreateOrderAsync(OrderRequest orderRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("orders", orderRequest);
        var content = await response.Content.ReadAsStringAsync();
        if (response.IsSuccessStatusCode)
        {
            return (JsonConvert.DeserializeObject<OrderResponse>(content), null);
        }
        return (null, content);
    }

    public async Task<Settings?> GetSettingsAsync()
    {
        var response = await _httpClient.GetAsync("settings");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<Settings>(json);
    }

    public async Task<bool> UpdateSettingsAsync(Settings settings)
    {
        var payload = new
        {
            TaxPercentage = settings.TaxPercentage,
            Currency = settings.Currency,
            SessionTimeoutMinutes = settings.SessionTimeoutMinutes
        };
        var response = await _httpClient.PutAsJsonAsync("settings", payload);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ChangePasswordAsync(string currentPassword, string newPassword)
    {
        var response = await _httpClient.PostAsJsonAsync("settings/change-credentials", new
        {
            CurrentPassword = currentPassword,
            NewPassword = newPassword
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ResetSystemAsync()
    {
        var response = await _httpClient.PostAsync("settings/reset", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<LowStockItem>> GetLowStockAsync()
    {
        var response = await _httpClient.GetAsync("menuitems/low-stock");
        if (!response.IsSuccessStatusCode) return new List<LowStockItem>();
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<List<LowStockItem>>(json) ?? new List<LowStockItem>();
    }

    public async Task<bool> AdjustStockAsync(int menuItemId, int quantityChange, string reason)
    {
        var response = await _httpClient.PostAsJsonAsync($"menuitems/{menuItemId}/adjust-stock", new
        {
            QuantityChange = quantityChange,
            Reason = reason
        });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RestoreBackupAsync(string filePath)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = File.OpenRead(filePath);
        content.Add(new StreamContent(stream), "file", Path.GetFileName(filePath));
        var response = await _httpClient.PostAsync("backup/restore", content);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateBackupAsync()
    {
        var response = await _httpClient.PostAsync("backup/create", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ExportOrdersCsvAsync(DateTime? startDate, DateTime? endDate, string savePath)
    {
        var query = new List<string>();
        if (startDate.HasValue) query.Add($"startDate={startDate:O}");
        if (endDate.HasValue) query.Add($"endDate={endDate:O}");
        var url = "analytics/export/csv";
        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        var response = await _httpClient.GetAsync(url);
        if (!response.IsSuccessStatusCode) return false;

        var bytes = await response.Content.ReadAsByteArrayAsync();
        await File.WriteAllBytesAsync(savePath, bytes);
        return true;
    }

    public async Task<DailyAnalytics?> GetDailyAnalyticsAsync(DateTime date)
    {
        var response = await _httpClient.GetAsync($"analytics/daily?date={date:O}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<DailyAnalytics>(json);
    }

    public async Task<WeeklyAnalytics?> GetWeeklyAnalyticsAsync(DateTime startDate)
    {
        var response = await _httpClient.GetAsync($"analytics/weekly?startDate={startDate:O}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<WeeklyAnalytics>(json);
    }

    public async Task<MonthlyAnalytics?> GetMonthlyAnalyticsAsync(int year, int month)
    {
        var response = await _httpClient.GetAsync($"analytics/monthly?year={year}&month={month}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<MonthlyAnalytics>(json);
    }

    public async Task<YearlyAnalytics?> GetYearlyAnalyticsAsync(int year)
    {
        var response = await _httpClient.GetAsync($"analytics/yearly?year={year}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<YearlyAnalytics>(json);
    }

    public async Task<ProfitAnalytics?> GetProfitAnalyticsAsync(DateTime? startDate, DateTime? endDate)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-30);
        var end = endDate ?? DateTime.UtcNow;
        var response = await _httpClient.GetAsync($"analytics/profit?startDate={start:O}&endDate={end:O}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<ProfitAnalytics>(json);
    }

    public async Task<BestSellersResponse?> GetBestSellersAsync(DateTime? startDate, DateTime? endDate, int top = 10)
    {
        var start = startDate ?? DateTime.UtcNow.AddDays(-7);
        var end = endDate ?? DateTime.UtcNow;
        var response = await _httpClient.GetAsync($"analytics/bestsellers?startDate={start:O}&endDate={end:O}&top={top}");
        if (!response.IsSuccessStatusCode) return null;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<BestSellersResponse>(json);
    }
}

