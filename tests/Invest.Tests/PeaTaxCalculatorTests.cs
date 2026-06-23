using Invest.Core.Calc;
using Xunit;

namespace Invest.Tests;

public class PeaTaxCalculatorTests
{
    private readonly PeaTaxCalculator _calc = new();

    [Fact]
    public void Avant5ans_ApplicationFlatTax30()
    {
        var open = new DateTime(2023, 1, 1);
        var withdrawal = new DateTime(2025, 1, 1); // 2 ans

        var r = _calc.ComputeWithdrawalTax(1000m, open, withdrawal);

        Assert.False(r.IsFiveYearsReached);
        Assert.Equal(0.30m, r.TaxRate);
        Assert.Equal(300m, r.TaxAmount);
        Assert.Equal(700m, r.NetGain);
    }

    [Fact]
    public void Apres5ans_SeulementPrelevementsSociaux172()
    {
        var open = new DateTime(2018, 1, 1);
        var withdrawal = new DateTime(2025, 1, 1); // 7 ans

        var r = _calc.ComputeWithdrawalTax(1000m, open, withdrawal);

        Assert.True(r.IsFiveYearsReached);
        Assert.Equal(0.172m, r.TaxRate);
        Assert.Equal(172m, r.TaxAmount);
        Assert.Equal(828m, r.NetGain);
    }

    [Fact]
    public void MoinsValue_AucunImpot()
    {
        var open = new DateTime(2024, 1, 1);
        var r = _calc.ComputeWithdrawalTax(-500m, open, new DateTime(2025, 1, 1));

        Assert.Equal(0m, r.TaxAmount);
        Assert.Equal(-500m, r.NetGain);
    }

    [Fact]
    public void Progression_BorneeEntre0Et1()
    {
        var open = new DateTime(2020, 1, 1);
        Assert.Equal(0.5, _calc.MaturityProgress(open, new DateTime(2022, 7, 2)), 1);
        Assert.Equal(1.0, _calc.MaturityProgress(open, new DateTime(2030, 1, 1)));
    }
}
