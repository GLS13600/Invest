# Invest 📈

Application de bureau **Windows** (WPF / .NET 8) de suivi d'investissements et de simulation
financière, en remplacement du prototype Excel `Suivi_Investissement.xlsm`.

Design **dark mode** épuré inspiré de **Finary** et **Trade Republic**. Données 100 % **locales**
(SQLite), prix en direct via **Yahoo Finance**.

---

## ✨ Fonctionnalités

- **Dashboard** : patrimoine net, variation, courbe de performance, cartes KPI
  (total investi, plus-value latente, plus-value nette après impôt), donut de répartition.
- **Réel** : journal de transactions (ajout/suppression), positions agrégées (PRU, +/- value).
- **Simulation** : sliders réactifs (croissance mensuelle, apport, horizon) avec recalcul
  instantané ; bouton **« Synchroniser avec le Réel »** (remplace la macro VBA *Reset*).
- **PEA** : jauge de progression des 5 ans, compte à rebours, simulateur de retrait
  (Flat Tax 30 % avant 5 ans / prélèvements sociaux 17,2 % après).
- **Budget** : trésorerie mensuelle (ex-feuille *Dépense 2026*), capacité d'épargne →
  apport suggéré pour la simulation.

## 🏗️ Architecture

```
Invest.sln
├─ src/Invest.Core/    Logique métier pure et testable (modèles, moteurs de calcul, fiscalité)
├─ src/Invest.Data/    Persistance EF Core + SQLite (DbContext, seed initial)
├─ src/Invest.App/     Interface WPF (MVVM) : thème dark, vues, graphiques custom
└─ tests/Invest.Tests/ Tests unitaires xUnit du moteur métier
```

| Brique | Technologie |
|---|---|
| Framework UI | .NET 8 + WPF |
| MVVM | CommunityToolkit.Mvvm |
| Base locale | SQLite + EF Core |
| Prix live | Yahoo Finance (HttpClient) |
| Graphiques | Contrôles custom (`AreaChart`, `DonutChart`) sans dépendance externe |

## 🚀 Démarrage (Windows requis pour WPF)

### Prérequis
- Windows 10/11
- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0)
- Visual Studio 2022 **ou** VS Code + extension **C# Dev Kit**

### Avec Visual Studio
1. Ouvrir `Invest.sln`
2. Définir **Invest.App** comme projet de démarrage
3. **F5**

### Avec VS Code
```bash
dotnet restore
dotnet build
dotnet run --project src/Invest.App
```
> ⚠️ La compilation de **Invest.App** (WPF) ne fonctionne que sous **Windows**.
> Les projets `Invest.Core` et `Invest.Tests` sont multiplateformes.

### Lancer les tests
```bash
dotnet test
```

## 💾 Données

La base est créée automatiquement au premier lancement dans
`%AppData%\Invest\invest.db`, pré-remplie avec quelques transactions réelles
(Exail, ArcelorMittal, ETF S&P 500 / MSCI World) reprises de l'Excel d'origine.

## 🗺️ Prochaines étapes

- Historique de valorisation persistant (snapshots quotidiens)
- Gestion CRUD des actifs et comptes depuis l'UI
- Export / import CSV
- Conversion de devises pour les actifs hors zone euro
