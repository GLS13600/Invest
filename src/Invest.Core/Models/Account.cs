namespace Invest.Core.Models;

/// <summary>
/// Un compte d'investissement (PEA, Compte-Titres, Livret...).
/// La date d'ouverture sert au compteur fiscal des 5 ans du PEA.
/// </summary>
public class Account
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public AccountType Type { get; set; }

    /// <summary>
    /// Date d'ouverture du compte = premier versement.
    /// Point de départ du décompte des 5 ans pour l'exonération PEA.
    /// </summary>
    public DateTime OpenDate { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
}
