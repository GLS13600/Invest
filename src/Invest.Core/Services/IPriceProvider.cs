namespace Invest.Core.Services;

/// <summary>Cotation renvoyée par une source de prix.</summary>
public readonly record struct Quote(string Ticker, decimal Price, string Currency, DateTime AsOf);

/// <summary>Source de prix en direct (Yahoo Finance par défaut, extensible).</summary>
public interface IPriceProvider
{
    /// <summary>Récupère la dernière cotation d'un symbole, ou null si indisponible.</summary>
    Task<Quote?> GetQuoteAsync(string ticker, CancellationToken ct = default);

    /// <summary>Récupère plusieurs cotations en une fois.</summary>
    Task<IReadOnlyList<Quote>> GetQuotesAsync(IEnumerable<string> tickers, CancellationToken ct = default);

    /// <summary>
    /// Cours de clôture historique d'un symbole à une date donnée (ou au dernier jour
    /// de bourse précédent si la date tombe un week-end / jour férié). Null si indisponible.
    /// </summary>
    Task<decimal?> GetHistoricalCloseAsync(string ticker, DateTime date, CancellationToken ct = default);
}
