using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using CamelliaPOS.WPF.Models;
using CamelliaPOS.WPF.ViewModels;

namespace CamelliaPOS.WPF.Views;

public partial class MainWindow : Window
{
    private DispatcherTimer? _timer;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel(App.ApiService);
        
        // Update time display
        _timer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _timer.Tick += (s, e) => TimeDisplay.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _timer.Start();
        TimeDisplay.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
    }

    private void MenuItem_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement element && element.DataContext is MenuItem menuItem)
        {
            if (DataContext is MainViewModel vm)
            {
                vm.AddToCartCommand.Execute(menuItem);
            }
        }
    }
}

