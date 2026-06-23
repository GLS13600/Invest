using Invest.Core.Models;

namespace Invest.Core.Calc;

/// <summary>Résultat d'une simulation de retrait fiscal.</summary>
public class WithdrawalTaxResult
{
    public decimal GrossGain { get; init; }
    public decimal TaxRate { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal NetGain { get; init; }
    public bool IsFiveYearsReached { get; init; }
    public double YearsHeld { get; init; }
}

/// <summary>
/// Fiscalité PEA. Règle des 5 ans :
///   - Retrait AVANT 5 ans  : Flat Tax 30 % sur la plus-value.
///   - Retrait APRÈS 5 ans  : seulement prélèvements sociaux 17,2 %.
/// Le décompte part de la date d'ouverture du PEA (premier versement).
/// </summary>
public class PeaTaxCalculator
{
    public const decimal FlatTaxRate = 0.30m;        // 12,8 % IR + 17,2 % PS
    public const decimal SocialChargesRate = 0.172m; // prélèvements sociaux seuls
    public const double MaturityYears = 5.0;

    /// <summary>Calcule l'impôt dû sur une plus-value en cas de retrait à une date donnée.</summary>
    public WithdrawalTaxResult ComputeWithdrawalTax(decimal gain, DateTime openDate, DateTime withdrawalDate)
    {
        double years = (withdrawalDate - openDate).TotalDays / 365.25;
        bool mature = years >= MaturityYears;

        // Aucune fiscalité sur une moins-value.
        decimal taxableGain = Math.Max(0m, gain);
        decimal rate = mature ? SocialChargesRate : FlatTaxRate;
        decimal tax = Math.Round(taxableGain * rate, 2);

        return new WithdrawalTaxResult
        {
            GrossGain = gain,
            TaxRate = rate,
            TaxAmount = tax,
            NetGain = gain - tax,
            IsFiveYearsReached = mature,
            YearsHeld = years
        };
    }

    /// <summary>Date à laquelle le PEA atteint ses 5 ans (exonération d'IR).</summary>
    public DateTime MaturityDate(DateTime openDate) => openDate.AddYears(5);

    /// <summary>Progression vers les 5 ans, bornée à [0,1], pour la jauge du dashboard.</summary>
    public double MaturityProgress(DateTime openDate, DateTime now)
    {
        double years = (now - openDate).TotalDays / 365.25;
        return Math.Clamp(years / MaturityYears, 0.0, 1.0);
    }

    /// <summary>Nombre de jours restants avant les 5 ans (0 si déjà atteint).</summary>
    public int DaysUntilMaturity(DateTime openDate, DateTime now)
    {
        var maturity = MaturityDate(openDate);
        return now >= maturity ? 0 : (int)Math.Ceiling((maturity - now).TotalDays);
    }
}
