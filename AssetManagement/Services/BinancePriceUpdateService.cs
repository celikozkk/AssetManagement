using System.Globalization;
using AssetManagement.Data;
using Microsoft.Extensions.Options;

namespace AssetManagement.Services;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

public class BinancePriceUpdateService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _updateInterval;
    private readonly BinanceSettings _binanceSettings;
    private readonly ILogger<BinancePriceUpdateService> _logger;

    public BinancePriceUpdateService(IServiceProvider serviceProvider, IOptions<BinanceSettings> binanceSettings, ILogger<BinancePriceUpdateService> logger)
    {
        _serviceProvider = serviceProvider;
        _binanceSettings = binanceSettings.Value;
        _updateInterval = TimeSpan.FromSeconds(_binanceSettings.UpdateIntervalSeconds);
        _logger = logger;
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdatePrices();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating prices from Binance.");
            }

            await Task.Delay(_updateInterval, stoppingToken);
        }
    }

    private async Task UpdatePrices()
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
}

public class BinancePrice
{
    public string symbol { get; set; }
    public string price { get; set; }
}