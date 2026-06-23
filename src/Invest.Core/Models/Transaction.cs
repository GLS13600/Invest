namespace Invest.Core.Models;

/// <summary>
/// Une ligne du journal "Achats|Ventes". Reproduit la logique de l'Excel :
/// ValeurInvestie = Prix * (Vente ? -Quantité : +Quantité) + Frais + Impôts.
/// </summary>
public class Transaction
{
    public int Id { get; set; }

    public DateTime Date { get; set; }

    public int AccountId { get; set; }
    public Account? Account { get; set; }

    public int AssetId { get; set; }
    public Asset? Asset { get; set; }

    public TransactionSide Side { get; set; }

    /// <summary>Prix unitaire payé/reçu.</summary>
    public decimal Price { get; set; }

    public decimal Quantity { get; set; }

    public decimal Fees { get; set; }

    /// <summary>Impôts/taxes éventuels prélevés à la transaction.</summary>
    public decimal Taxes { get; set; }

    /// <summary>Quantité signée : négative pour une vente.</summary>
    public decimal SignedQuantity => Side == TransactionSide.Vente ? -Quantity : Quantity;

    /// <summary>
    /// Montant réellement engagé/récupéré, identique à la colonne "Valeurs investi" de l'Excel.
    /// Positif = argent sorti (achat), négatif = argent rentré (vente).
    /// </summary>
    public decimal InvestedValue => Price * SignedQuantity + Fees + Taxes;
}
