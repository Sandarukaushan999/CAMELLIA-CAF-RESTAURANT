using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CamelliaPOS.WPF.Services;
using CamelliaPOS.WPF.Views;

namespace CamelliaPOS.WPF.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _apiService;

    [ObservableProperty]
    private string username = string.Empty;

    [ObservableProperty]
    private string password = string.Empty;

    [ObservableProperty]
    private string errorMessage = string.Empty;

    [ObservableProperty]
    private bool isLoading = false;

    public LoginViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    [RelayCommand]
    private async Task LoginAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ErrorMessage = "Please enter username and password";
            return;
        }

        IsLoading = true;
        ErrorMessage = string.Empty;

        try
        {
            var response = await _apiService.LoginAsync(Username, Password);
            
            if (response != null)
            {
                // Navigate to main POS window
                var mainWindow = new MainWindow();
                mainWindow.Show();
                
                // Close login window
                Application.Current.Windows.OfType<LoginWindow>().FirstOrDefault()?.Close();
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}

