using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;

namespace Invest.Core.Services;

/// <summary>
/// Récupère les prix via l'endpoint public de Yahoo Finance (gratuit, non officiel).
/// Exemple : https://query1.finance.yahoo.com/v8/finance/chart/EXA.PA?interval=1d&range=1d
/// En cas d'échec réseau, renvoie null : l'appli retombe alors sur la saisie manuelle.
/// </summary>
public class YahooPriceProvider : IPriceProvider
{
    private const string BaseUrl = "https://query1.finance.yahoo.com/v8/finance/chart/";
    private readonly HttpClient _http;

    public YahooPriceProvider(HttpClient http)
    {
        _http = http;
        if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
        {
            // Yahoo refuse les requêtes sans User-Agent.
            _http.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Invest/1.0");
        }
    }

    public async Task<Quote?> GetQuoteAsync(string ticker, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(ticker)) return null;

        try
        {
            var url = $"{BaseUrl}{Uri.EscapeDataString(ticker)}?interval=1d&range=1d";
            using var doc = await _http.GetFromJsonAsync<JsonDocument>(url, ct);
            if (doc is null) return null;

            var meta = doc.RootElement
                .GetProperty("chart")
                .GetProperty("result")[0]
                .GetProperty("meta");

            decimal price = meta.GetProperty("regularMarketPrice").GetDecimal();
            string currency = meta.TryGetProperty("currency", out var c)
                ? c.GetString() ?? "EUR"
                : "EUR";

            return new Quote(ticker, price, currency, DateTime.Now);
        }
        catch
        {
            // Réseau indisponible / symbole inconnu / format inattendu -> fallback manuel.
            return null;
        }
    }

    public async Task<IReadOnlyList<Quote>> GetQuotesAsync(IEnumerable<string> tickers, CancellationToken ct = default)
    {
        var tasks = tickers
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Distinct()
            .Select(t => GetQuoteAsync(t, ct));

        var results = await Task.WhenAll(tasks);
        return results.Where(q => q.HasValue).Select(q => q!.Value).ToList();
    }
}
