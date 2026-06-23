using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invest.App.Controls;
using Invest.App.Services;
using Invest.Core.Calc;

namespace Invest.App.ViewModels;

public partial class DashboardViewModel : ObservableObject, ILoadable
{
    private readonly PortfolioService _service;
    private readonly PortfolioEngine _portfolio;
    private readonly PeaTaxCalculator _tax;

    // Palette de teintes pour le donut.
    private static readonly Color[] Palette =
    {
        Color.FromRgb(0x7C, 0x5C, 0xFC), Color.FromRgb(0x1F, 0xD2, 0x86),
        Color.FromRgb(0x36, 0xB3, 0xFF), Color.FromRgb(0xFF, 0xB1, 0x4D),
        Color.FromRgb(0xFF, 0x5C, 0x6C), Color.FromRgb(0x9B, 0xA3, 0xB4)
    };

    [ObservableProperty] private decimal _netWorth;
    [ObservableProperty] private decimal _invested;
    [ObservableProperty] private decimal _latentGain;
    [ObservableProperty] private decimal _netGainAfterTax;
    [ObservableProperty] private double _gainPercent;
    [ObservableProperty] private string _taxNote = "";
    [ObservableProperty] private IReadOnlyList<double>? _series;
    [ObservableProperty] private IReadOnlyList<DonutSlice>? _allocation;
    [ObservableProperty] private string _allocationLegend = "";
    [ObservableProperty] private bool _isBusy;

    public DashboardViewModel(PortfolioService service, PortfolioEngine portfolio, PeaTaxCalculator tax)
    {
        _service = service;
        _portfolio = portfolio;
        _tax = tax;
    }

    public Task LoadAsync()
    {
        var holdings = _service.GetHoldings();

        Invested = _portfolio.TotalCostBasis(holdings);
        NetWorth = _portfolio.TotalMarketValue(holdings);
        LatentGain = _portfolio.TotalGain(holdings);
        GainPercent = Invested != 0 ? (double)(LatentGain / Invested) : 0;

        // Plus-value nette : on applique la fiscalité PEA au cas d'un retrait aujourd'hui.
        var pea = _service.GetPeaAccount();
        if (pea is not null && LatentGain > 0)
        {
            var r = _tax.ComputeWithdrawalTax(LatentGain, pea.OpenDate, DateTime.Now);
            NetGainAfterTax = r.NetGain;
            TaxNote = r.IsFiveYearsReached
                ? "après prélèvements sociaux 17,2 %"
                : "après Flat Tax 30 % (PEA < 5 ans)";
        }
        else
        {
            NetGainAfterTax = LatentGain;
            TaxNote = "—";
        }

        Series = _service.GetValueSeries();

        // Répartition par actif pour le donut + légende.
        var slices = _portfolio.Allocation(holdings, h => h.Asset.Name);
        Allocation = slices.Select((s, i) => new DonutSlice
        {
            Label = s.Label,
            Value = (double)s.Value,
            Color = Palette[i % Palette.Length]
        }).ToList();
        AllocationLegend = string.Join("   ",
            slices.Take(5).Select(s => $"{s.Label} {s.Percent:P0}"));

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task RefreshPrices()
    {
        IsBusy = true;
        try
        {
            await _service.RefreshPricesAsync();
            await LoadAsync();
        }
        finally { IsBusy = false; }
    }
}
