using System.Windows;
using CamelliaPOS.WPF.Services;

namespace CamelliaPOS.WPF;

public partial class App : Application
{
    public static ApiService ApiService { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApiService = new ApiService();
    }
}
