using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using Invest.App.Services;
using Invest.Core.Calc;

namespace Invest.App.ViewModels;

public partial class BudgetViewModel : ObservableObject, ILoadable
{
    private readonly PortfolioService _service;

    public ObservableCollection<MonthlyBudget> Months { get; } = new();

    [ObservableProperty] private int _year = 2026;
    [ObservableProperty] private decimal _totalIncome;
    [ObservableProperty] private decimal _totalExpenses;
    [ObservableProperty] private decimal _totalSavings;
    [ObservableProperty] private decimal _capacityForInvesting;

    public BudgetViewModel(PortfolioService service) => _service = service;

    public Task LoadAsync()
    {
        var months = _service.GetBudget(Year);
        Months.Clear();
        foreach (var m in months) Months.Add(m);

        TotalIncome = months.Sum(m => m.Income);
        TotalExpenses = months.Sum(m => m.Expenses);
        TotalSavings = months.Sum(m => m.Savings);
        // Capacité d'épargne moyenne -> sert d'apport mensuel suggéré à la simulation.
        CapacityForInvesting = months.Count > 0
            ? Math.Round(months.Average(m => m.Savings), 2)
            : 0m;

        return Task.CompletedTask;
    }
}
