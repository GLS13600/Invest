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

    // Réévalue l'état du bouton "Ajouter" quand le formulaire change.
    partial void OnNewAssetChanged(Asset? value) => AddTransactionCommand.NotifyCanExecuteChanged();
    partial void OnNewAccountChanged(Account? value) => AddTransactionCommand.NotifyCanExecuteChanged();
    partial void OnNewPriceChanged(decimal value) => AddTransactionCommand.NotifyCanExecuteChanged();
    partial void OnNewQuantityChanged(decimal value) => AddTransactionCommand.NotifyCanExecuteChanged();
}
