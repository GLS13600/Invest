namespace Invest.Core.Models;

/// <summary>
/// Historique de valorisation du patrimoine, utilisé pour tracer la courbe du dashboard.
/// Un point par jour (ou par capture).
/// </summary>
public class PriceSnapshot
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    /// <summary>Valeur totale du portefeuille à cette date.</summary>
    public decimal PortfolioValue { get; set; }

    /// <summary>Total réellement investi (cumul des sorties d'argent) à cette date.</summary>
    public decimal InvestedValue { get; set; }
}
