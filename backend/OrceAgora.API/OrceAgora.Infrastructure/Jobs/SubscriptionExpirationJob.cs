using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrceAgora.Domain.Enums;
using OrceAgora.Infrastructure.Data;

namespace OrceAgora.Infrastructure.Jobs;

public class SubscriptionExpirationJob(
    IServiceScopeFactory scopeFactory,
    ILogger<SubscriptionExpirationJob> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var db = scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

                var today = DateOnly.FromDateTime(DateTime.UtcNow);

                // Busca assinaturas com cancelamento agendado e período expirado
                var expired = await db.Subscriptions
                    .Include(s => s.User)
                    .Where(s => s.CancelAtPeriodEnd &&
                                s.CurrentPeriodEnd < today &&
                                s.Plan == "pro")
                    .ToListAsync(stoppingToken);

                foreach (var sub in expired)
                {
                    sub.Plan = "basic";
                    sub.Status = "cancelled";
                    sub.User.Plan = PlanType.Basic;
                    logger.LogInformation(
                        "Assinatura expirada para usuário {UserId}", sub.UserId);
                }

                if (expired.Any())
                    await db.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro no job de expiração");
            }

            // Roda a cada hora
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
        }
    }
}