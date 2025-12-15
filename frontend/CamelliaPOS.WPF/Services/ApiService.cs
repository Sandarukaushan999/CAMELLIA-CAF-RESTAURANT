using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Newtonsoft.Json;
using CamelliaPOS.WPF.Models;

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

    public async Task<OrderResponse?> CreateOrderAsync(OrderRequest orderRequest)
    {
        var response = await _httpClient.PostAsJsonAsync("orders", orderRequest);
        if (response.IsSuccessStatusCode)
        {
            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<OrderResponse>(json);
        }
        return null;
    }
}

