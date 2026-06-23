using Invest.Core.Models;

namespace Invest.Core.Calc;

/// <summary>Synthèse d'un mois de budget.</summary>
public readonly record struct MonthlyBudget(
    int Month,
    decimal Income,
    decimal Expenses,
    decimal Savings,
    decimal Net,
    decimal RunningBalance);

/// <summary>
/// Reproduit la logique de trésorerie de "Dépense 2026" :
///   Net du mois     = Revenus - Dépenses - Épargne
///   Solde reporté   = solde précédent + net (cf. C88 = B88 + C82)
///   Capacité d'épargne = part allouée à l'investissement, alimente le simulateur.
/// </summary>
public class BudgetEngine
{
    public IReadOnlyList<MonthlyBudget> BuildYear(IEnumerable<BudgetEntry> entries, int year, decimal openingBalance = 0m)
    {
        var byMonth = entries.Where(e => e.Year == year).ToLookup(e => e.Month);
        var result = new List<MonthlyBudget>(12);
        decimal balance = openingBalance;

        for (int month = 1; month <= 12; month++)
        {
            var rows = byMonth[month].ToList();
            decimal income = rows.Where(r => r.Kind == BudgetKind.Revenu).Sum(r => r.Amount);
            decimal expenses = rows.Where(r => r.Kind == BudgetKind.Depense).Sum(r => r.Amount);
            decimal savings = rows.Where(r => r.Kind == BudgetKind.Epargne).Sum(r => r.Amount);
            decimal net = income - expenses - savings;
            balance += net;

            result.Add(new MonthlyBudget(month, income, expenses, savings, net, balance));
        }

        return result;
    }

    /// <summary>Capacité d'épargne moyenne mensuelle dégagée sur l'année (pour pré-remplir l'apport simulé).</summary>
    public decimal AverageMonthlySavings(IEnumerable<BudgetEntry> entries, int year)
    {
        var months = BuildYear(entries, year);
        return months.Count == 0 ? 0m : Math.Round(months.Average(m => m.Savings), 2);
    }
}
