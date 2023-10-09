using AssetManagement.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace AssetManagement.Services;

public class NotificationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEmailService _emailService;
    private readonly TimeSpan _checkInterval;
    private readonly ILogger<NotificationService> _logger;
    private readonly NotificationSettings _notificationSettings;


    public NotificationService(IServiceProvider serviceProvider, IEmailService emailService, ILogger<NotificationService> logger, IOptions<NotificationSettings> notificationSettings)
    {
        _serviceProvider = serviceProvider;
        _emailService = emailService;
        _logger = logger;
        _notificationSettings = notificationSettings.Value;
        _checkInterval = TimeSpan.FromSeconds(_notificationSettings.CheckIntervalSeconds);
    }
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckAndNotifyUsers();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while checking and notifying users.");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }
    }

    private async Task CheckAndNotifyUsers()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<TradingContext>();

        var positions = context.Positions.Include(p => p.Account).Include(p => p.Asset).ToList();

        // Group positions by AccountId
        var groupedPositions = positions.GroupBy(p => p.AccountId);

        foreach (var group in groupedPositions)
        {
            var accountId = group.Key;
            var userPositions = group.ToList();
            if (userPositions.Count == 0)
                continue;
            
            var account = userPositions.First().Account;

            decimal totalCurrentValue = 0;
            decimal totalEntryValue = 0; 

            foreach (var position in userPositions)
            {
                totalCurrentValue += position.Amount * position.Asset.LastPrice;
                totalEntryValue += position.Amount * position.EntryPrice;
            }
            
            var changePercent = ((totalCurrentValue - totalEntryValue) / totalEntryValue) * 100;

            if (account.NotificationRate > 0 && Math.Abs(changePercent) >= Convert.ToDecimal(account.NotificationRate))
            {
                var direction = changePercent > 0 ? "increased" : "decreased";
                var subject = $"The total value of your positions has {direction}!";
                var content = $"The total value of your positions has {direction} by {changePercent}%. Current value: {totalCurrentValue}. Previous value: {totalEntryValue}.";
                    
                await _emailService.SendEmailAsync(account.Email, subject, content);
            }
        }
    }
}
