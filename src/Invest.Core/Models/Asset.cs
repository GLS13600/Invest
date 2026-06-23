namespace Invest.Core.Models;

/// <summary>
/// Un actif négociable (action, ETF...). Le Ticker est l'identifiant Yahoo Finance
/// utilisé pour récupérer le prix en direct (ex. "EXA.PA" pour Exail Technologies).
/// </summary>
public class Asset
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Symbole Yahoo Finance (ex: "EXA.PA", "MC.PA", "CW8.PA").</summary>
    public string Ticker { get; set; } = string.Empty;

    public AssetClass Class { get; set; }

    /// <summary>Pays du siège social, utilisé pour la répartition géographique.</summary>
    public string Country { get; set; } = "France";

    public string Currency { get; set; } = "EUR";

    /// <summary>Dernier prix connu (mis à jour par le provider de prix ou saisi à la main).</summary>
    public decimal LastPrice { get; set; }

    public DateTime? LastPriceUpdate { get; set; }

    public List<Transaction> Transactions { get; set; } = new();
}
