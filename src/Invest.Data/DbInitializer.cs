using Invest.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Invest.Data;

/// <summary>
/// Crée la base au premier lancement et y injecte un jeu de données initial,
/// repris des transactions réelles du fichier Excel (Exail, ArcelorMittal, ETF...).
/// </summary>
public static class DbInitializer
{
    public static void Initialize(InvestDbContext db)
    {
        db.Database.EnsureCreated();
        if (db.Assets.Any()) return; // déjà initialisée

        var pea = new Account { Name = "PEA", Type = AccountType.PEA, OpenDate = new DateTime(2023, 3, 14) };
        var cto = new Account { Name = "Compte-Titres", Type = AccountType.CompteTitres, OpenDate = new DateTime(2024, 9, 1) };
        db.Accounts.AddRange(pea, cto);

        var exail = new Asset { Name = "Exail Technologies", Ticker = "EXA.PA", Class = AssetClass.Action, Country = "France", LastPrice = 126.40m };
        var arcelor = new Asset { Name = "ArcelorMittal", Ticker = "MT.AS", Class = AssetClass.Action, Country = "Luxembourg", LastPrice = 60.64m };
        var sp500 = new Asset { Name = "BNPP Easy S&P 500 ETF", Ticker = "ESE.PA", Class = AssetClass.ETF, Country = "US", LastPrice = 32.955m };
        var world = new Asset { Name = "Amundi PEA MSCI World", Ticker = "CW8.PA", Class = AssetClass.ETF, Country = "Monde", LastPrice = 5.991m };
        db.Assets.AddRange(exail, arcelor, sp500, world);

        db.SaveChanges();

        // Transactions PEA réelles (extraites du journal "Achats|Ventes").
        db.Transactions.AddRange(
            Tx(new DateTime(2026, 1, 6), pea, exail, TransactionSide.Achat, 109.60m, 2, 1m, 0.88m),
            Tx(new DateTime(2026, 1, 6), pea, exail, TransactionSide.Achat, 110.00m, 1, 0.55m, 0.44m),
            Tx(new DateTime(2026, 3, 4), pea, exail, TransactionSide.Achat, 120.60m, 1, 0.60m, 0.48m),
            Tx(new DateTime(2026, 3, 16), pea, exail, TransactionSide.Achat, 141.20m, 1, 0.71m, 0.56m),
            Tx(new DateTime(2026, 5, 4), pea, exail, TransactionSide.Achat, 119.80m, 1, 0.60m, 0.48m),
            Tx(new DateTime(2026, 5, 12), pea, exail, TransactionSide.Achat, 114.60m, 1, 0.57m, 0.46m),

            Tx(new DateTime(2026, 1, 6), pea, arcelor, TransactionSide.Achat, 40.67m, 2, 0.41m, 0m),
            Tx(new DateTime(2026, 3, 4), pea, arcelor, TransactionSide.Achat, 49.93m, 1, 0.25m, 0m),
            Tx(new DateTime(2026, 5, 26), pea, arcelor, TransactionSide.Achat, 61.40m, 4, 1m, 0m),

            Tx(new DateTime(2026, 1, 6), pea, sp500, TransactionSide.Achat, 29.99m, 3, 0.45m, 0m),
            Tx(new DateTime(2026, 1, 10), pea, sp500, TransactionSide.Achat, 29.96m, 1, 0.15m, 0m),
            Tx(new DateTime(2026, 2, 1), pea, sp500, TransactionSide.Achat, 29.72m, 2, 0.30m, 0m),
            Tx(new DateTime(2026, 2, 4), pea, sp500, TransactionSide.Achat, 29.05m, 5, 0.73m, 0m),
            Tx(new DateTime(2026, 5, 12), pea, sp500, TransactionSide.Achat, 32.51m, 1, 0.16m, 0m),
            Tx(new DateTime(2026, 5, 27), pea, sp500, TransactionSide.Achat, 32.91m, 4, 0.66m, 0m),

            Tx(new DateTime(2026, 1, 12), pea, world, TransactionSide.Achat, 5.58m, 17, 0.47m, 0m),
            Tx(new DateTime(2026, 2, 2), pea, world, TransactionSide.Achat, 5.50m, 8, 0.22m, 0m),
            Tx(new DateTime(2026, 3, 4), pea, world, TransactionSide.Achat, 5.50m, 15, 0.41m, 0m),
            Tx(new DateTime(2026, 6, 1), pea, world, TransactionSide.Achat, 6.02m, 3, 0.09m, 0m)
        );

        // Budget mensuel de démarrage (feuille "Dépense 2026"), répliqué pour Jan-Mai.
        for (int month = 1; month <= 5; month++)
        {
            db.BudgetEntries.AddRange(
                Budget(month, "Salaire", BudgetKind.Revenu, 1450m),
                Budget(month, "Loyer", BudgetKind.Depense, 580m),
                Budget(month, "Courses", BudgetKind.Depense, 70m),
                Budget(month, "Salle de sport", BudgetKind.Depense, 24.99m),
                Budget(month, "Spotify", BudgetKind.Depense, 7.07m),
                Budget(month, "Investissement", BudgetKind.Epargne, 300m)
            );
        }

        db.Scenarios.Add(new SimulationScenario
        {
            Name = "Scénario agressif",
            MonthlyGrowthRate = 0.10,
            MonthlyContribution = 300m,
            Months = 60
        });

        db.SaveChanges();
    }

    private static Transaction Tx(DateTime date, Account acc, Asset asset, TransactionSide side,
        decimal price, decimal qty, decimal fees, decimal taxes) => new()
    {
        Date = date,
        Account = acc,
        Asset = asset,
        Side = side,
        Price = price,
        Quantity = qty,
        Fees = fees,
        Taxes = taxes
    };

    private static BudgetEntry Budget(int month, string cat, BudgetKind kind, decimal amount) => new()
    {
        Year = 2026,
        Month = month,
        Category = cat,
        Kind = kind,
        Amount = amount
    };
}
