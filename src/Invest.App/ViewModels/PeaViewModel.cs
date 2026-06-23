using CommunityToolkit.Mvvm.ComponentModel;
using Invest.App.Services;
using Invest.Core.Calc;
using Invest.Core.Models;

namespace Invest.App.ViewModels;

public partial class PeaViewModel : ObservableObject, ILoadable
{
    private readonly PortfolioService _service;
    private readonly PeaTaxCalculator _tax;
    private Account? _pea;

    [ObservableProperty] private double _progress;        // 0..1 pour la jauge
    [ObservableProperty] private int _daysRemaining;
    [ObservableProperty] private DateTime _maturityDate;
    [ObservableProperty] private bool _isMature;
    [ObservableProperty] private string _statusText = "";

    // Simulateur de retrait
    [ObservableProperty] private decimal _withdrawalGain = 1000m;
    [ObservableProperty] private decimal _estimatedTax;
    [ObservableProperty] private decimal _netAfterTax;
    [ObservableProperty] private string _taxRateText = "";

    public PeaViewModel(PortfolioService service, PeaTaxCalculator tax)
    {
        _service = service;
        _tax = tax;
    }

    public Task LoadAsync()
    {
        _pea = _service.GetPeaAccount();
        if (_pea is null)
        {
            StatusText = "Aucun PEA enregistré.";
            return Task.CompletedTask;
        }

        var now = DateTime.Now;
        Progress = _tax.MaturityProgress(_pea.OpenDate, now);
        DaysRemaining = _tax.DaysUntilMaturity(_pea.OpenDate, now);
        MaturityDate = _tax.MaturityDate(_pea.OpenDate);
        IsMature = DaysRemaining == 0;

        StatusText = IsMature
            ? "PEA mature : plus-values exonérées d'impôt (hors prélèvements sociaux 17,2 %)."
            : $"Ouvert le {_pea.OpenDate:dd/MM/yyyy} — exonération d'IR le {MaturityDate:dd/MM/yyyy}.";

        Recompute();
        return Task.CompletedTask;
    }

    private void Recompute()
    {
        if (_pea is null) return;
        var r = _tax.ComputeWithdrawalTax(WithdrawalGain, _pea.OpenDate, DateTime.Now);
        EstimatedTax = r.TaxAmount;
        NetAfterTax = r.NetGain;
        TaxRateText = $"{r.TaxRate:P1} " + (r.IsFiveYearsReached ? "(prélèvements sociaux)" : "(Flat Tax)");
    }

    partial void OnWithdrawalGainChanged(decimal value) => Recompute();
}
