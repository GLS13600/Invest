using Invest.Core.Calc;
using Invest.Core.Models;
using Invest.Core.Services;
using Invest.Data;
using Microsoft.EntityFrameworkCore;

namespace Invest.App.Services;

/// <summary>
/// Façade entre les écrans et la base : charge les données, déclenche les calculs
/// (portefeuille, budget) et rafraîchit les prix via Yahoo. Source de vérité unique de l'UI.
/// </summary>
public class PortfolioService
{
    private readonly IDbContextFactory<InvestDbContext> _dbFactory;
    private readonly IPriceProvider _prices;
    private readonly PortfolioEngine _portfolio;
    private readonly BudgetEngine _budget;

    public PortfolioService(
        IDbContextFactory<InvestDbContext> dbFactory,
        IPriceProvider prices,
        PortfolioEngine portfolio,
        BudgetEngine budget)
    {
        _dbFactory = dbFactory;
        _prices = prices;
        _portfolio = portfolio;
        _budget = budget;
    }

    // ----------------------------- Lectures -----------------------------

    public List<Transaction> GetTransactions()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Transactions
            .Include(t => t.Asset)
            .Include(t => t.Account)
            .OrderByDescending(t => t.Date)
            .ToList();
    }

    public List<Asset> GetAssets()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Assets.OrderBy(a => a.Name).ToList();
    }

    public List<Account> GetAccounts()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Accounts.OrderBy(a => a.Name).ToList();
    }

    public IReadOnlyList<Holding> GetHoldings()
    {
        using var db = _dbFactory.CreateDbContext();
        var tx = db.Transactions.Include(t => t.Asset).ToList();
        return _portfolio.BuildHoldings(tx);
    }

    /// <summary>
    /// Série de valorisation du portefeuille dans le temps : à chaque date de transaction,
    /// quantités cumulées valorisées au prix courant de chaque actif. Sert à tracer la courbe.
    /// </summary>
    public IReadOnlyList<double> GetValueSeries()
    {
        using var db = _dbFactory.CreateDbContext();
        var tx = db.Transactions.Include(t => t.Asset)
            .OrderBy(t => t.Date).ToList();
        if (tx.Count == 0) return Array.Empty<double>();

        var runningQty = new Dictionary<int, decimal>();
        var series = new List<double>();

        foreach (var t in tx)
        {
            runningQty.TryGetValue(t.AssetId, out var q);
            runningQty[t.AssetId] = q + t.SignedQuantity;

            decimal value = runningQty.Sum(kv =>
            {
                var price = tx.First(x => x.AssetId == kv.Key).Asset!.LastPrice;
                return kv.Value * price;
            });
            series.Add((double)value);
        }

        return series;
    }

    public IReadOnlyList<MonthlyBudget> GetBudget(int year)
    {
        using var db = _dbFactory.CreateDbContext();
        var entries = db.BudgetEntries.Where(e => e.Year == year).ToList();
        return _budget.BuildYear(entries, year);
    }

    public Account? GetPeaAccount()
    {
        using var db = _dbFactory.CreateDbContext();
        return db.Accounts.FirstOrDefault(a => a.Type == AccountType.PEA);
    }

    public SimulationScenario GetOrCreateScenario()
    {
        using var db = _dbFactory.CreateDbContext();
        var s = db.Scenarios.FirstOrDefault();
        if (s is null)
        {
            s = new SimulationScenario();
            db.Scenarios.Add(s);
            db.SaveChanges();
        }
        return s;
    }

    // ----------------------------- Écritures -----------------------------

    public void AddTransaction(Transaction tx)
    {
        using var db = _dbFactory.CreateDbContext();
        // Seuls les FK (AssetId/AccountId) sont renseignés : Add n'insère pas d'entité liée.
        db.Transactions.Add(tx);
        db.SaveChanges();
    }

    public void DeleteTransaction(int id)
    {
        using var db = _dbFactory.CreateDbContext();
        var tx = db.Transactions.Find(id);
        if (tx is not null)
        {
            db.Transactions.Remove(tx);
            db.SaveChanges();
        }
    }

    public void SaveScenario(SimulationScenario scenario)
    {
        using var db = _dbFactory.CreateDbContext();
        db.Scenarios.Update(scenario);
        db.SaveChanges();
    }

    /// <summary>Cours de clôture historique d'un actif à une date (pour pré-remplir un ordre).</summary>
    public async Task<decimal?> GetHistoricalPriceAsync(int assetId, DateTime date, CancellationToken ct = default)
    {
        string? ticker;
        using (var db = _dbFactory.CreateDbContext())
            ticker = db.Assets.Where(a => a.Id == assetId).Select(a => a.Ticker).FirstOrDefault();

        if (string.IsNullOrWhiteSpace(ticker)) return null;
        return await _prices.GetHistoricalCloseAsync(ticker, date, ct);
    }

    /// <summary>
    /// Met à jour le prix de chaque transaction avec le cours historique de sa date d'achat.
    /// Renvoie le nombre d'ordres mis à jour. Les ordres dont le cours est introuvable sont conservés.
    /// </summary>
    public async Task<int> UpdateOrderPricesFromDateAsync(CancellationToken ct = default)
    {
        using var db = _dbFactory.CreateDbContext();
        var transactions = db.Transactions.Include(t => t.Asset).ToList();

        int updated = 0;
        foreach (var tx in transactions)
        {
            if (tx.Asset is null || string.IsNullOrWhiteSpace(tx.Asset.Ticker)) continue;
            var price = await _prices.GetHistoricalCloseAsync(tx.Asset.Ticker, tx.Date, ct);
            if (price is > 0)
            {
                tx.Price = price.Value;
                updated++;
            }
        }
        if (updated > 0) db.SaveChanges();
        return updated;
    }

    /// <summary>
    /// Récupère les prix live Yahoo et met à jour LastPrice. Renvoie le nombre d'actifs mis à jour.
    /// En cas d'échec (réseau coupé), les prix manuels existants sont conservés.
    /// </summary>
    public async Task<int> RefreshPricesAsync(CancellationToken ct = default)
    {
        using var db = _dbFactory.CreateDbContext();
        var assets = db.Assets.Where(a => a.Ticker != "").ToList();
        var quotes = await _prices.GetQuotesAsync(assets.Select(a => a.Ticker), ct);
        var map = quotes.ToDictionary(q => q.Ticker, StringComparer.OrdinalIgnoreCase);

        int updated = 0;
        foreach (var asset in assets)
        {
            if (map.TryGetValue(asset.Ticker, out var q))
            {
                asset.LastPrice = q.Price;
                asset.LastPriceUpdate = q.AsOf;
                updated++;
            }
        }
        if (updated > 0) db.SaveChanges();
        return updated;
    }
}
