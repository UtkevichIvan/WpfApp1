using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Windows;


namespace WpfApp1;
public partial class App : Application
{
    private IHost _host;

    public App()
    {
        _host = new HostBuilder()
                    .ConfigureServices((context, services) =>
                    {
                        services.AddSingleton<MainWindow>();
                        services.AddHostedService<ReaderService>();
                    })
                    .Build();
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();
        var mainWindow = _host.Services.GetService<MainWindow>();
        mainWindow.Show();
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }
}