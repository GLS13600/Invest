using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Invest.App.Services;
using Invest.Core.Calc;
using Invest.Core.Models;

namespace Invest.App.ViewModels;

public partial class ReelViewModel : ObservableObject, ILoadable
{
    private readonly PortfolioService _service;

    public ObservableCollection<Transaction> Transactions { get; } = new();
    public ObservableCollection<Holding> Holdings { get; } = new();
    public ObservableCollection<Asset> Assets { get; } = new();
    public ObservableCollection<Account> Accounts { get; } = new();

    public TransactionSide[] Sides { get; } =
        { TransactionSide.Achat, TransactionSide.Vente, TransactionSide.Bonus };

    // --- Champs du formulaire d'ajout ---
    [ObservableProperty] private DateTime _newDate = DateTime.Today;
    [ObservableProperty] private Asset? _newAsset;
    [ObservableProperty] private Account? _newAccount;
    [ObservableProperty] private TransactionSide _newSide = TransactionSide.Achat;
    [ObservableProperty] private decimal _newPrice;
    [ObservableProperty] private decimal _newQuantity;
    [ObservableProperty] private decimal _newFees;
    [ObservableProperty] private bool _isBusy;
    [ObservableProperty] private string _status = "";

    public ReelViewModel(PortfolioService service) => _service = service;

    public Task LoadAsync()
    {
        Transactions.Clear();
        foreach (var t in _service.GetTransactions()) Transactions.Add(t);

        Holdings.Clear();
        foreach (var h in _service.GetHoldings()) Holdings.Add(h);

        Assets.Clear();
        foreach (var a in _service.GetAssets()) Assets.Add(a);
        NewAsset ??= Assets.FirstOrDefault();

        Accounts.Clear();
        foreach (var a in _service.GetAccounts()) Accounts.Add(a);
        NewAccount ??= Accounts.FirstOrDefault();

        return Task.CompletedTask;
    }

    private bool CanAdd() => NewAsset is not null && NewAccount is not null
        && NewPrice > 0 && NewQuantity > 0;

    [RelayCommand(CanExecute = nameof(CanAdd))]
    private async Task AddTransaction()
    {
        _service.AddTransaction(new Transaction
        {
            Date = NewDate,
            AssetId = NewAsset!.Id,
            AccountId = NewAccount!.Id,
            Side = NewSide,
            Price = NewPrice,
            Quantity = NewQuantity,
            Fees = NewFees,
            Taxes = 0m
        });

        NewPrice = 0; NewQuantity = 0; NewFees = 0;
        await LoadAsync();
    }

    [RelayCommand]
    private async Task DeleteTransaction(Transaction? tx)
    {
        if (tx is null) return;
        _service.DeleteTransaction(tx.Id);
        await LoadAsync();
    }

    /// <summary>Renseigne le prix avec le cours historique de l'actif à la date choisie.</summary>
    [RelayCommand]
    private async Task FetchPriceForDate()
    {
        if (NewAsset is null) return;
        IsBusy = true;
        Status = "Récupération du cours…";
        try
        {
            var price = await _service.GetHistoricalPriceAsync(NewAsset.Id, NewDate);
            if (price is > 0)
            {
                NewPrice = decimal.Round(price.Value, 4);
                Status = $"Cours du {NewDate:dd/MM/yyyy} : {NewPrice:N2}";
            }
            else
            {
                Status = "Cours indisponible pour cette date — saisis le prix manuellement.";
            }
        }
        finally { IsBusy = false; }
    }

    /// <summary>Met à jour tous les ordres existants au cours historique de leur date d'achat.</summary>
    [RelayCommand]
    private async Task UpdateOrderPrices()
    {
        IsBusy = true;
        Status = "Mise à jour des prix d'achat…";
        try
        {
            int n = await _service.UpdateOrderPricesFromDateAsync();
            Status = n > 0 ? $"{n} ordre(s) mis à jour." : "Aucun prix mis à jour (réseau ?).";
            await LoadAsync();
        }
        finally { IsBusy = false; }
    }

    // Réévalue l'état du bouton "Ajouter" quand le formulaire change.
    partial void OnNewAssetChanged(Asset? value)
    {
        AddTransactionCommand.NotifyCanExecuteChanged();
        _ = FetchPriceForDate(); // remplissage auto du prix à la sélection de l'actif
    }
    partial void OnNewDateChanged(DateTime value) => _ = FetchPriceForDate();
    partial void OnNewAccountChanged(Account? value) => AddTransactionCommand.NotifyCanExecuteChanged();
    partial void OnNewPriceChanged(decimal value) => AddTransactionCommand.NotifyCanExecuteChanged();
    partial void OnNewQuantityChanged(decimal value) => AddTransactionCommand.NotifyCanExecuteChanged();
}
