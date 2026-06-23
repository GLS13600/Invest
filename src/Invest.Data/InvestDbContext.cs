using Invest.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Invest.Data;

/// <summary>Contexte EF Core. Toutes les données restent locales dans un fichier SQLite.</summary>
public class InvestDbContext : DbContext
{
    public InvestDbContext(DbContextOptions<InvestDbContext> options) : base(options) { }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<PriceSnapshot> PriceSnapshots => Set<PriceSnapshot>();
    public DbSet<BudgetEntry> BudgetEntries => Set<BudgetEntry>();
    public DbSet<SimulationScenario> Scenarios => Set<SimulationScenario>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        // SQLite ne gère pas nativement decimal : on stocke en TEXT pour préserver la précision.
        foreach (var property in b.Model.GetEntityTypes()
                     .SelectMany(t => t.GetProperties())
                     .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
        {
            property.SetColumnType("TEXT");
        }

        b.Entity<Asset>().HasIndex(a => a.Ticker);

        b.Entity<Transaction>()
            .HasOne(t => t.Asset)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Entity<Transaction>()
            .HasOne(t => t.Account)
            .WithMany(a => a.Transactions)
            .HasForeignKey(t => t.AccountId)
            .OnDelete(DeleteBehavior.Restrict);

        // Propriétés calculées : pas de colonne en base.
        b.Entity<Transaction>().Ignore(t => t.SignedQuantity).Ignore(t => t.InvestedValue);
        b.Entity<BudgetEntry>().Ignore(e => e.SignedAmount);
    }
}
