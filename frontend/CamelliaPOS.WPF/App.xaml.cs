using System.Windows;
using CamelliaPOS.WPF.Services;

namespace CamelliaPOS.WPF;

public partial class App : Application
{
    public static ApiService ApiService { get; private set; } = null!;
    public static string CurrentRole { get; set; } = string.Empty;
    public static string CurrentUsername { get; set; } = string.Empty;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ApiService = new ApiService();
    }
}
