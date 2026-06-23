using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Invest.App.Services;
using Invest.App.ViewModels;
using Invest.Core.Calc;
using Invest.Core.Services;
using Invest.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Invest.App;

public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // --- Base de données locale (SQLite dans %AppData%/Invest) ---
                var dir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Invest");
                Directory.CreateDirectory(dir);
                var dbPath = Path.Combine(dir, "invest.db");

                services.AddDbContextFactory<InvestDbContext>(o => o.UseSqlite($"Data Source={dbPath}"));

                // --- Source de prix Yahoo Finance ---
                services.AddHttpClient<IPriceProvider, YahooPriceProvider>();

                // --- Moteurs métier ---
                services.AddSingleton<PortfolioEngine>();
                services.AddSingleton<SimulationEngine>();
                services.AddSingleton<PeaTaxCalculator>();
                services.AddSingleton<BudgetEngine>();

                // --- Service applicatif ---
                services.AddSingleton<PortfolioService>();

                // --- ViewModels ---
                services.AddSingleton<MainViewModel>();
                services.AddTransient<DashboardViewModel>();
                services.AddTransient<ReelViewModel>();
                services.AddTransient<SimulationViewModel>();
                services.AddTransient<PeaViewModel>();
                services.AddTransient<BudgetViewModel>();

                services.AddSingleton<MainWindow>();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        // Affichage des nombres/dates en français.
        var fr = CultureInfo.GetCultureInfo("fr-FR");
        CultureInfo.DefaultThreadCurrentCulture = fr;
        CultureInfo.DefaultThreadCurrentUICulture = fr;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(fr.IetfLanguageTag)));

        // Création + seed de la base au premier lancement.
        var dbFactory = _host.Services.GetRequiredService<IDbContextFactory<InvestDbContext>>();
        using (var db = dbFactory.CreateDbContext())
        {
            DbInitializer.Initialize(db);
        }

        var window = _host.Services.GetRequiredService<MainWindow>();
        window.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        window.Show();

        await _host.StartAsync();
        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await _host.StopAsync();
        _host.Dispose();
        base.OnExit(e);
    }
}
