using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invest.App.Services;
using Invest.Core.Calc;
using Invest.Core.Models;

namespace Invest.App.ViewModels;

public partial class SimulationViewModel : ObservableObject, ILoadable
{
    private readonly PortfolioService _service;
    private readonly SimulationEngine _engine;
    private SimulationScenario _scenario = new();

    // Les sliders bindent ces propriétés -> recalcul instantané (voir On...Changed).
    [ObservableProperty] private double _monthlyGrowthPercent = 10;   // 0..30 %
    [ObservableProperty] private decimal _monthlyContribution = 300m;
    [ObservableProperty] private int _months = 60;
    [ObservableProperty] private decimal _startingValue;

    [ObservableProperty] private IReadOnlyList<double>? _series;
    [ObservableProperty] private decimal _finalValue;
    [ObservableProperty] private decimal _totalInvested;
    [ObservableProperty] private decimal _projectedGain;
    [ObservableProperty] private double _annualizedPercent;

    public SimulationViewModel(PortfolioService service, SimulationEngine engine)
    {
        _service = service;
        _engine = engine;
    }

    public Task LoadAsync()
    {
        _scenario = _service.GetOrCreateScenario();
        MonthlyGrowthPercent = _scenario.MonthlyGrowthRate * 100;
        MonthlyContribution = _scenario.MonthlyContribution;
        Months = _scenario.Months;

        if (StartingValue == 0) SyncWithReal();
        Recompute();
        return Task.CompletedTask;
    }

    /// <summary>Équivalent du bouton VBA "Reset" : recharge la valeur réelle du portefeuille.</summary>
    [RelayCommand]
    private void SyncWithReal()
    {
        var holdings = _service.GetHoldings();
        StartingValue = holdings.Sum(h => h.MarketValue);
        Recompute();
    }

    private void Recompute()
    {
        double rate = MonthlyGrowthPercent / 100.0;
        var points = _engine.Project(StartingValue, rate, MonthlyContribution, Months);

        Series = points.Select(p => (double)p.PortfolioValue).ToList();
        var last = points[^1];
        FinalValue = last.PortfolioValue;
        TotalInvested = last.Invested;
        ProjectedGain = last.PortfolioValue - last.Invested;
        AnnualizedPercent = SimulationEngine.AnnualizedRate(rate);

        // Persiste les réglages du scénario.
        _scenario.MonthlyGrowthRate = rate;
        _scenario.MonthlyContribution = MonthlyContribution;
        _scenario.Months = Months;
        _scenario.StartingValue = StartingValue;
        _service.SaveScenario(_scenario);
    }

    // Réactivité instantanée des sliders.
    partial void OnMonthlyGrowthPercentChanged(double value) => Recompute();
    partial void OnMonthlyContributionChanged(decimal value) => Recompute();
    partial void OnMonthsChanged(int value) => Recompute();
}
