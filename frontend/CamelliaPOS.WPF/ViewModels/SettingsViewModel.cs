using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CamelliaPOS.WPF.Models;
using CamelliaPOS.WPF.Services;
using System.Windows;

namespace CamelliaPOS.WPF.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private Settings settings = new();

    [ObservableProperty]
    private string currentPassword = string.Empty;

    [ObservableProperty]
    private string newPassword = string.Empty;

    [ObservableProperty]
    private bool isLoading;

    public SettingsViewModel(ApiService apiService)
    {
        _apiService = apiService;
        LoadAsync();
    }

    private async void LoadAsync()
    {
        IsLoading = true;
        try
        {
            var s = await _apiService.GetSettingsAsync();
            if (s != null)
            {
                Settings = s;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        IsLoading = true;
        try
        {
            var ok = await _apiService.UpdateSettingsAsync(Settings);
            if (ok)
            {
                MessageBox.Show("Settings saved.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to save settings.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword))
        {
            MessageBox.Show("Enter current and new passwords.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsLoading = true;
        try
        {
            var ok = await _apiService.ChangePasswordAsync(CurrentPassword, NewPassword);
            if (ok)
            {
                MessageBox.Show("Password changed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
            }
            else
            {
                MessageBox.Show("Failed to change password.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error changing password: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ResetSystemAsync()
    {
        if (MessageBox.Show("This will clear all sales, order and waste records. Continue?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;

        IsLoading = true;
        try
        {
            var ok = await _apiService.ResetSystemAsync();
            if (ok)
            {
                MessageBox.Show("System reset completed.", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Failed to reset system.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error resetting system: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
}


