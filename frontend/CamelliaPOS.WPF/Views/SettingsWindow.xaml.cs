using System.Windows;
using CamelliaPOS.WPF.ViewModels;

namespace CamelliaPOS.WPF.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
    }

    private void ChangePassword_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is SettingsViewModel vm)
        {
            vm.CurrentPassword = CurrentPasswordBox.Password;
            vm.NewPassword = NewPasswordBox.Password;
            vm.ChangePasswordCommand.Execute(null);
        }
    }
}


