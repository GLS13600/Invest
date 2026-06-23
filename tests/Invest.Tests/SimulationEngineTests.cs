using Invest.Core.Calc;
using Xunit;

namespace Invest.Tests;

public class SimulationEngineTests
{
    private readonly SimulationEngine _engine = new();

    [Fact]
    public void Projection_CapitaliseEtAjouteApport()
    {
        // 1000€, +10%/mois, +100€/mois, 1 mois => 1000*1.1 + 100 = 1200
        var points = _engine.Project(1000m, 0.10, 100m, 1);

        Assert.Equal(2, points.Count);
        Assert.Equal(1000m, points[0].PortfolioValue);
        Assert.Equal(1200m, points[1].PortfolioValue);
    }

    [Fact]
    public void TotalInvesti_NeCumuleQueLesApports()
    {
        var points = _engine.Project(1000m, 0.20, 50m, 3);

        // Investi = départ + 3 apports
        Assert.Equal(1150m, points[^1].Invested);
        // La valeur projetée doit dépasser le seul investi (croissance positive)
        Assert.True(points[^1].PortfolioValue > points[^1].Invested);
    }

    [Fact]
    public void TauxAnnualise_CorrespondAuComposeMensuel()
    {
        // (1+0.10)^12 - 1 ≈ 2.138
        Assert.Equal(2.138, SimulationEngine.AnnualizedRate(0.10), 3);
    }
}
