using System.Windows;
using CamelliaPOS.WPF.ViewModels;

namespace CamelliaPOS.WPF.Views;

public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
        DataContext = new LoginViewModel(App.ApiService);
        
        // Bind password box
        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        };
    }
}

