using System.Globalization;
using AssetManagement.Data;
using Microsoft.Extensions.Options;

namespace AssetManagement.Binance;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class BinancePriceUpdateService : IHostedService, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private Timer _timer;
    private readonly BinanceSettings _binanceSettings;

    public BinancePriceUpdateService(IServiceProvider serviceProvider, IOptions<BinanceSettings> binanceSettings)
    {
        _serviceProvider = serviceProvider;
        _binanceSettings = binanceSettings.Value;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(UpdatePrices, null, TimeSpan.Zero,
            TimeSpan.FromSeconds(_binanceSettings.UpdateIntervalSeconds));
        return Task.CompletedTask;
    }

    private async void UpdatePrices(object state)
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TradingContext>();

        var symbols = context.Assets.Select(a => $"\"{a.Symbol}USDT\"").ToList();
        var symbolsQueryString = string.Join(",", symbols);
        var encodedSymbols = System.Net.WebUtility.UrlEncode($"[{symbolsQueryString}]");

        var apiUrl = $"{_binanceSettings.ApiBaseUrl}{encodedSymbols}";

        var httpClient = new HttpClient();
        var response = await httpClient.GetStringAsync(apiUrl);
        var prices = JsonSerializer.Deserialize<List<BinancePrice>>(response);

        foreach (var price in prices)
        {
            var asset = context.Assets.FirstOrDefault(a =>
                a.Symbol == price.symbol.Substring(0, price.symbol.Length - 4)); // Removing "USDT"
            if (asset != null)
            {
                asset.LastPrice = decimal.Parse(price.price, CultureInfo.InvariantCulture);
            }
        }

        await context.SaveChangesAsync();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}

public class BinancePrice
{
    public string symbol { get; set; }
    public string price { get; set; }
}