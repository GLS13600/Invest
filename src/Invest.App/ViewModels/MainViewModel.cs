using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Invest.App.ViewModels;

/// <summary>Pilote la navigation latérale et héberge le ViewModel courant.</summary>
public partial class MainViewModel : ObservableObject
{
    private readonly DashboardViewModel _dashboard;
    private readonly ReelViewModel _reel;
    private readonly SimulationViewModel _simulation;
    private readonly PeaViewModel _pea;
    private readonly BudgetViewModel _budget;

    [ObservableProperty]
    private ObservableObject? _current;

    [ObservableProperty]
    private string _selected = "Dashboard";

    public MainViewModel(
        DashboardViewModel dashboard,
        ReelViewModel reel,
        SimulationViewModel simulation,
        PeaViewModel pea,
        BudgetViewModel budget)
    {
        _dashboard = dashboard;
        _reel = reel;
        _simulation = simulation;
        _pea = pea;
        _budget = budget;

        Navigate("Dashboard");
    }

    [RelayCommand]
    private void Navigate(string target)
    {
        Selected = target;
        Current = target switch
        {
            "Dashboard" => _dashboard,
            "Reel" => _reel,
            "Simulation" => _simulation,
            "Pea" => _pea,
            "Budget" => _budget,
            _ => _dashboard
        };

        // Recharge les données à chaque entrée sur l'écran.
        if (Current is ILoadable loadable)
            _ = loadable.LoadAsync();
    }
}

/// <summary>Écran capable de (re)charger ses données à l'affichage.</summary>
public interface ILoadable
{
    Task LoadAsync();
}
