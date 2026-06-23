using Invest.Core.Models;

namespace Invest.Core.Calc;

/// <summary>
/// Agrège les transactions en positions, reproduisant les formules du "Tableau de bord" :
///   Quantité  = SUMPRODUCT(achats - ventes)
///   CostBasis = SUMIF(Valeurs investi)   (PRU)
///   Gain      = ValeurLive - CostBasis
/// </summary>
public class PortfolioEngine
{
    /// <summary>Construit les positions par actif à partir d'un journal de transactions.</summary>
    public IReadOnlyList<Holding> BuildHoldings(IEnumerable<Transaction> transactions)
    {
        var holdings = transactions
            .Where(t => t.Asset is not null)
            .GroupBy(t => t.Asset!)
            .Select(g => new Holding
            {
                Asset = g.Key,
                Quantity = g.Sum(t => t.SignedQuantity),
                CostBasis = g.Sum(t => t.InvestedValue),
                CurrentPrice = g.Key.LastPrice
            })
            // On ignore les positions totalement soldées (quantité ~ 0).
            .Where(h => Math.Abs(h.Quantity) > 0.0000001m)
            .OrderByDescending(h => h.MarketValue)
            .ToList();

        return holdings;
    }

    /// <summary>Valeur de marché totale du portefeuille (le "Patrimoine Net" hors cash).</summary>
    public decimal TotalMarketValue(IEnumerable<Holding> holdings) => holdings.Sum(h => h.MarketValue);

    /// <summary>Total réellement investi (somme des prix de revient des positions ouvertes).</summary>
    public decimal TotalCostBasis(IEnumerable<Holding> holdings) => holdings.Sum(h => h.CostBasis);

    /// <summary>Plus-value latente totale.</summary>
    public decimal TotalGain(IEnumerable<Holding> holdings) => holdings.Sum(h => h.Gain);

    /// <summary>
    /// Répartition en pourcentage selon un sélecteur de clé (par actif, par pays, par compte...).
    /// </summary>
    public IReadOnlyList<AllocationSlice> Allocation(
        IEnumerable<Holding> holdings,
        Func<Holding, string> keySelector)
    {
        var list = holdings.ToList();
        var total = list.Sum(h => h.MarketValue);
        if (total == 0) return Array.Empty<AllocationSlice>();

        return list
            .GroupBy(keySelector)
            .Select(g => new AllocationSlice
            {
                Label = g.Key,
                Value = g.Sum(h => h.MarketValue),
                Percent = g.Sum(h => h.MarketValue) / total
            })
            .OrderByDescending(s => s.Value)
            .ToList();
    }
}

/// <summary>Une part du camembert de répartition.</summary>
public class AllocationSlice
{
    public string Label { get; init; } = string.Empty;
    public decimal Value { get; init; }
    public decimal Percent { get; init; }
}
