using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CamelliaPOS.WPF.Models;
using CamelliaPOS.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CamelliaPOS.WPF.ViewModels;

public partial class AnalyticsViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private DateTime selectedDate = DateTime.Today;
    [ObservableProperty] private DateTime rangeStart = DateTime.Today.AddDays(-7);
    [ObservableProperty] private DateTime rangeEnd = DateTime.Today;
    [ObservableProperty] private int year = DateTime.Today.Year;
    [ObservableProperty] private int month = DateTime.Today.Month;

    [ObservableProperty] private DailyAnalytics? daily;
    [ObservableProperty] private WeeklyAnalytics? weekly;
    [ObservableProperty] private MonthlyAnalytics? monthlyData;
    [ObservableProperty] private YearlyAnalytics? yearlyData;
    [ObservableProperty] private ProfitAnalytics? profit;
    [ObservableProperty] private BestSellersResponse? bestSellers;

    [ObservableProperty] private bool isLoading;

    public ObservableCollection<HourlySale> Hourly => new(daily?.HourlySales ?? new());
    public ObservableCollection<ItemStat> TopItems => new(daily?.TopItems ?? new());
    public ObservableCollection<ItemStat> LeastItems => new(daily?.LeastItems ?? new());

    public AnalyticsViewModel(ApiService api)
    {
        _api = api;
        _ = LoadAllAsync();
    }

    [RelayCommand]
    private async Task LoadAllAsync()
    {
        IsLoading = true;
        try
        {
            Daily = await _api.GetDailyAnalyticsAsync(SelectedDate);
            Weekly = await _api.GetWeeklyAnalyticsAsync(RangeStart);
            MonthlyData = await _api.GetMonthlyAnalyticsAsync(Year, Month);
            YearlyData = await _api.GetYearlyAnalyticsAsync(Year);
            Profit = await _api.GetProfitAnalyticsAsync(RangeStart, RangeEnd);
            BestSellers = await _api.GetBestSellersAsync(RangeStart, RangeEnd, 10);

            OnPropertyChanged(nameof(Hourly));
            OnPropertyChanged(nameof(TopItems));
            OnPropertyChanged(nameof(LeastItems));
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading analytics: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}


