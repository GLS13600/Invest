namespace Invest.Core.Models;

/// <summary>
/// Paramètres d'un scénario de simulation. Remplace la feuille "Simulation" + la macro VBA Reset :
/// le bouton "Synchroniser avec le Réel" recharge la valeur de départ depuis le portefeuille réel.
/// </summary>
public class SimulationScenario
{
    public int Id { get; set; }

    public string Name { get; set; } = "Scénario";

    /// <summary>Taux de croissance mensuel simulé (ex: 0.10 = +10%/mois). Réglé par le slider.</summary>
    public double MonthlyGrowthRate { get; set; } = 0.10;

    /// <summary>Apport mensuel ajouté au portefeuille (DCA).</summary>
    public decimal MonthlyContribution { get; set; } = 200m;

    /// <summary>Valeur de départ du portefeuille (importée du réel lors de la synchronisation).</summary>
    public decimal StartingValue { get; set; }

    /// <summary>Horizon de la projection en mois.</summary>
    public int Months { get; set; } = 60;
}
