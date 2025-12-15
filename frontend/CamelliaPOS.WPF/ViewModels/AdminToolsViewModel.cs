using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CamelliaPOS.WPF.Services;
using CamelliaPOS.WPF.Views;
using Microsoft.Win32;
using System.Windows;

namespace CamelliaPOS.WPF.ViewModels;

public partial class AdminToolsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private bool isBusy;

    public AdminToolsViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private void OpenSettings()
    {
        var vm = new SettingsViewModel(_apiService);
        var window = new SettingsWindow { DataContext = vm };
        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
        {
            window.Owner = Application.Current.MainWindow;
        }
        window.ShowDialog();
    }

    [RelayCommand]
    private void OpenLowStock()
    {
        var vm = new LowStockViewModel(_apiService);
        var window = new LowStockWindow { DataContext = vm };
        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
        {
            window.Owner = Application.Current.MainWindow;
        }
        window.ShowDialog();
    }

    [RelayCommand]
    private void OpenInventory()
    {
        var vm = new InventoryViewModel(_apiService);
        var window = new InventoryWindow { DataContext = vm };
        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
        {
            window.Owner = Application.Current.MainWindow;
        }
        window.ShowDialog();
    }

    [RelayCommand]
    private void OpenAnalytics()
    {
        var vm = new AnalyticsViewModel(_apiService);
        var window = new AnalyticsWindow { DataContext = vm };
        if (Application.Current.MainWindow != null && Application.Current.MainWindow.IsLoaded)
        {
            window.Owner = Application.Current.MainWindow;
        }
        window.ShowDialog();
    }

    [RelayCommand]
    private async Task CreateBackupAsync()
    {
        IsBusy = true;
        try
        {
            var ok = await _apiService.CreateBackupAsync();
            MessageBox.Show(ok ? "Backup created." : "Failed to create backup.", ok ? "Info" : "Error",
                MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error creating backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task RestoreBackupAsync()
    {
        var dlg = new OpenFileDialog
        {
            Filter = "Backup files (*.zip)|*.zip|All files (*.*)|*.*"
        };
        if (dlg.ShowDialog() != true) return;

        IsBusy = true;
        try
        {
            var ok = await _apiService.RestoreBackupAsync(dlg.FileName);
            MessageBox.Show(ok ? "Backup restored." : "Failed to restore backup.", ok ? "Info" : "Error",
                MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error restoring backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        var saveDialog = new SaveFileDialog
        {
            Filter = "CSV files (*.csv)|*.csv",
            FileName = $"orders_{DateTime.Now:yyyyMMdd}.csv"
        };
        if (saveDialog.ShowDialog() != true) return;

        IsBusy = true;
        try
        {
            var ok = await _apiService.ExportOrdersCsvAsync(null, null, saveDialog.FileName);
            MessageBox.Show(ok ? "Export completed." : "Failed to export.", ok ? "Info" : "Error",
                MessageBoxButton.OK, ok ? MessageBoxImage.Information : MessageBoxImage.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error exporting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }
}


