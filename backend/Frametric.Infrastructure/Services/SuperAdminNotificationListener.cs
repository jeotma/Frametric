using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Frametric.Application.Interfaces;
using Frametric.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Frametric.Infrastructure.Services;

public class SuperAdminNotificationListener : BackgroundService
{
    private readonly IConfiguration _configuration;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SuperAdminNotificationListener> _logger;

    public SuperAdminNotificationListener(
        IConfiguration configuration,
        IServiceScopeFactory scopeFactory,
        ILogger<SuperAdminNotificationListener> logger)
    {
        _configuration = configuration;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var connectionString = _configuration.GetConnectionString("DirectConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            _logger.LogWarning("Neither DirectConnection nor DefaultConnection is configured. SuperAdmin DB notification listener will not start.");
            return;
        }

        _logger.LogInformation("SuperAdmin notification background listener is starting.");

        try
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString)
            {
                KeepAlive = 30
            };
            connectionString = builder.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not set Npgsql KeepAlive in connection string. Proceeding with default.");
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                using var conn = new NpgsqlConnection(connectionString);
                await conn.OpenAsync(cancellationToken);

                conn.Notification += async (sender, e) =>
                {
                    try
                    {
                        await HandleNotificationAsync(e.Payload, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error handling SuperAdmin promotion notification.");
                    }
                };

                using (var cmd = new NpgsqlCommand("LISTEN superadmin_promoted;", conn))
                {
                    await cmd.ExecuteNonQueryAsync(cancellationToken);
                }

                _logger.LogInformation("Successfully listening to PostgreSQL channel: superadmin_promoted");

                // Keep waiting for notifications until cancelled
                while (!cancellationToken.IsCancellationRequested)
                {
                    await conn.WaitAsync(cancellationToken);
                }
            }
            catch (Exception ex)
            {
                if (cancellationToken.IsCancellationRequested) break;
                _logger.LogWarning(ex, "Lost connection to PostgreSQL. Reconnecting in 10 seconds...");
                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
            }
        }
    }

    private async Task HandleNotificationAsync(string payload, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received DB notification on channel 'superadmin_promoted' with payload: {Payload}", payload);

        Guid userId;
        string promotedBy = "Database SQL Client";

        if (Guid.TryParse(payload, out var parsedGuid))
        {
            userId = parsedGuid;
        }
        else
        {
            // Attempt to parse JSON payload
            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;
                if (root.TryGetProperty("UserId", out var userIdProp) && Guid.TryParse(userIdProp.GetString(), out var id))
                {
                    userId = id;
                }
                else
                {
                    _logger.LogWarning("Could not parse UserId from JSON payload: {Payload}", payload);
                    return;
                }

                if (root.TryGetProperty("PromotedBy", out var promotedByProp))
                {
                    promotedBy = promotedByProp.GetString() ?? promotedBy;
                }
            }
            catch (JsonException)
            {
                _logger.LogWarning("Payload is not a valid GUID or JSON string: {Payload}", payload);
                return;
            }
        }

        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<IApplicationDbContext>();
        var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();

        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} was promoted to SuperAdmin in database, but not found in DbContext.", userId);
            return;
        }

        if (user.SuperAdminNotificationSent)
        {
            _logger.LogInformation("SuperAdmin notification already sent for user {Username}. Skipping email.", user.Username);
            return;
        }

        // Send email
        await emailService.SendPromotionNotificationAsync(user.Username, user.Email, "SuperAdmin", promotedBy);

        // Mark notification as sent in the database
        user.SetSuperAdminNotificationSent(true);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
