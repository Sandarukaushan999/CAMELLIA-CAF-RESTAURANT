using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CamelliaPOS.WPF.Models;
using CamelliaPOS.WPF.Services;
using System.Collections.ObjectModel;
using System.Windows;

namespace CamelliaPOS.WPF.ViewModels;

public partial class LowStockViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private ObservableCollection<LowStockItem> items = new();

    [ObservableProperty]
    private LowStockItem? selectedItem;

    [ObservableProperty]
    private int adjustQuantity;

    [ObservableProperty]
    private string reason = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public LowStockViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadAsync();
    }

    private async void LoadAsync()
    {
        IsLoading = true;
        try
        {
            var list = await _apiService.GetLowStockAsync();
            Items.Clear();
            foreach (var i in list)
            {
                Items.Add(i);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading low stock items: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ApplyAdjustmentAsync()
    {
        if (SelectedItem == null)
        {
            MessageBox.Show("Select an item.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (AdjustQuantity == 0)
        {
            MessageBox.Show("Enter adjustment quantity (positive or negative).", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(Reason))
        {
            MessageBox.Show("Enter a reason.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var ok = await _apiService.AdjustStockAsync(SelectedItem.Id, AdjustQuantity, Reason);
            if (ok)
            {
                MessageBox.Show("Stock adjusted.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadAsync();
                AdjustQuantity = 0;
                Reason = string.Empty;
            }
            else
            {
                MessageBox.Show("Failed to adjust stock.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error adjusting stock: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}


