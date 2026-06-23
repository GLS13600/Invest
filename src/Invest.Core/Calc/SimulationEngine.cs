using Invest.Core.Models;

namespace Invest.Core.Calc;

/// <summary>Un point de la courbe de projection.</summary>
public readonly record struct ProjectionPoint(int MonthIndex, decimal PortfolioValue, decimal Invested);

/// <summary>
/// Projection en croissance composée, équivalent agressif de la feuille "Simulation".
/// Chaque mois :  valeur = valeur * (1 + taux) + apport.
/// Le total investi ne cumule que l'argent réellement ajouté (apports), pas la performance.
/// </summary>
public class SimulationEngine
{
    /// <summary>
    /// Calcule la trajectoire mensuelle du portefeuille.
    /// </summary>
    /// <param name="startingValue">Valeur de départ (souvent synchronisée depuis le réel).</param>
    /// <param name="monthlyGrowthRate">Taux mensuel, ex. 0.10 pour +10%/mois (slider).</param>
    /// <param name="monthlyContribution">Apport ajouté chaque mois.</param>
    /// <param name="months">Nombre de mois projetés.</param>
    public IReadOnlyList<ProjectionPoint> Project(
        decimal startingValue,
        double monthlyGrowthRate,
        decimal monthlyContribution,
        int months)
    {
        var points = new List<ProjectionPoint>(months + 1);

        decimal value = startingValue;
        decimal invested = startingValue;

        points.Add(new ProjectionPoint(0, value, invested));

        for (int m = 1; m <= months; m++)
        {
            // Capitalisation de l'existant puis ajout de l'apport du mois.
            value = value * (1m + (decimal)monthlyGrowthRate) + monthlyContribution;
            invested += monthlyContribution;
            points.Add(new ProjectionPoint(m, value, invested));
        }

        return points;
    }

    public IReadOnlyList<ProjectionPoint> Project(SimulationScenario s) =>
        Project(s.StartingValue, s.MonthlyGrowthRate, s.MonthlyContribution, s.Months);

    /// <summary>Taux annuel équivalent à un taux mensuel composé (pour l'affichage).</summary>
    public static double AnnualizedRate(double monthlyRate) =>
        Math.Pow(1 + monthlyRate, 12) - 1;
}
