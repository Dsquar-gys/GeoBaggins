using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GeoBaggins.AdminApp.Data;
using GeoBaggins.AdminApp.ViewModels;
using GeoBaggins.AdminApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GeoBaggins.AdminApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseNpgsql("Host=localhost;Port=5432;Database=GeoBaggins;Username=BagginsAdmin;Password=baggins;"));
        
        var serviceProvider = services.BuildServiceProvider();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(serviceProvider),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}