namespace Invest.Core.Models;

/// <summary>Type de compte. Le PEA possède la barrière fiscale des 5 ans.</summary>
public enum AccountType
{
    PEA,
    CompteTitres,
    Livret,
    Crypto
}

/// <summary>Sens d'une transaction, repris du journal "Achats|Ventes" de l'Excel.</summary>
public enum TransactionSide
{
    Achat,
    Vente,
    Bonus
}

/// <summary>Grande famille d'un actif (sert à la coloration / au filtrage).</summary>
public enum AssetClass
{
    Action,
    ETF,
    Obligation,
    Crypto,
    Cash
}

/// <summary>Nature d'une ligne de budget mensuel (feuille "Dépense 2026").</summary>
public enum BudgetKind
{
    Revenu,
    Depense,
    Epargne
}
