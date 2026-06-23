using Invest.Core.Calc;
using Invest.Core.Models;
using Xunit;

namespace Invest.Tests;

public class PortfolioEngineTests
{
    private readonly PortfolioEngine _engine = new();

    private static Asset Exail() => new() { Id = 1, Name = "Exail", LastPrice = 120m };

    [Fact]
    public void Position_AgregeAchatsEtVentes()
    {
        var exail = Exail();
        var tx = new List<Transaction>
        {
            new() { Asset = exail, AssetId = 1, Side = TransactionSide.Achat, Price = 100m, Quantity = 2, Fees = 1m },
            new() { Asset = exail, AssetId = 1, Side = TransactionSide.Achat, Price = 110m, Quantity = 1, Fees = 1m },
            new() { Asset = exail, AssetId = 1, Side = TransactionSide.Vente, Price = 115m, Quantity = 1, Fees = 1m },
        };

        var holdings = _engine.BuildHoldings(tx);
        var h = Assert.Single(holdings);

        // Quantité = 2 + 1 - 1 = 2
        Assert.Equal(2m, h.Quantity);
        // CostBasis = (200+1) + (110+1) + (-115+1) = 198
        Assert.Equal(198m, h.CostBasis);
        // Valeur de marché = 120 * 2 = 240
        Assert.Equal(240m, h.MarketValue);
        // Plus-value = 240 - 198 = 42
        Assert.Equal(42m, h.Gain);
    }

    [Fact]
    public void PositionSoldee_EstIgnoree()
    {
        var exail = Exail();
        var tx = new List<Transaction>
        {
            new() { Asset = exail, AssetId = 1, Side = TransactionSide.Achat, Price = 100m, Quantity = 1 },
            new() { Asset = exail, AssetId = 1, Side = TransactionSide.Vente, Price = 130m, Quantity = 1 },
        };

        Assert.Empty(_engine.BuildHoldings(tx));
    }

    [Fact]
    public void Allocation_SommeA100Pourcent()
    {
        var a = new Asset { Id = 1, Name = "A", LastPrice = 100m };
        var b = new Asset { Id = 2, Name = "B", LastPrice = 100m };
        var tx = new List<Transaction>
        {
            new() { Asset = a, AssetId = 1, Side = TransactionSide.Achat, Price = 100m, Quantity = 3 },
            new() { Asset = b, AssetId = 2, Side = TransactionSide.Achat, Price = 100m, Quantity = 1 },
        };

        var holdings = _engine.BuildHoldings(tx);
        var alloc = _engine.Allocation(holdings, h => h.Asset.Name);

        Assert.Equal(1m, alloc.Sum(s => s.Percent));
        Assert.Equal(0.75m, alloc.First(s => s.Label == "A").Percent);
    }
}
