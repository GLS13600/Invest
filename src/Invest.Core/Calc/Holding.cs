using Invest.Core.Models;

namespace Invest.Core.Calc;

/// <summary>
/// Position agrégée sur un actif, équivalent d'une ligne du "Tableau de bord" de l'Excel.
/// </summary>
public class Holding
{
    public Asset Asset { get; init; } = default!;

    /// <summary>Quantité nette détenue (achats - ventes).</summary>
    public decimal Quantity { get; init; }

    /// <summary>Prix de revient (cumul des "Valeurs investi"), équivalent du PRU.</summary>
    public decimal CostBasis { get; init; }

    /// <summary>Prix d'achat moyen = CostBasis / Quantité.</summary>
    public decimal AveragePrice => Quantity != 0 ? CostBasis / Quantity : 0m;

    /// <summary>Prix de marché courant utilisé pour la valorisation.</summary>
    public decimal CurrentPrice { get; init; }

    /// <summary>Valeur de marché = prix courant * quantité.</summary>
    public decimal MarketValue => CurrentPrice * Quantity;

    /// <summary>Plus/moins-value latente.</summary>
    public decimal Gain => MarketValue - CostBasis;

    public decimal GainPercent => CostBasis != 0 ? Gain / CostBasis : 0m;
}
