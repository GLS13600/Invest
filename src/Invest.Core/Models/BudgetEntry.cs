namespace Invest.Core.Models;

/// <summary>
/// Une ligne de budget mensuel (feuille "Dépense 2026") : salaire, loyer, abonnement, épargne...
/// La capacité d'épargne dégagée alimente le simulateur d'investissement.
/// </summary>
public class BudgetEntry
{
    public int Id { get; set; }

    public int Year { get; set; }

    /// <summary>Mois de 1 (Janvier) à 12 (Décembre).</summary>
    public int Month { get; set; }

    public string Category { get; set; } = string.Empty;

    public BudgetKind Kind { get; set; }

    /// <summary>Montant en valeur absolue (le signe est déduit du <see cref="Kind"/>).</summary>
    public decimal Amount { get; set; }

    /// <summary>Montant signé : revenu positif, dépense/épargne négatifs sur la trésorerie.</summary>
    public decimal SignedAmount => Kind == BudgetKind.Revenu ? Amount : -Amount;
}
