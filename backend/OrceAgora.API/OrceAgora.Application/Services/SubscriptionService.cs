using System.Text.Json;
using OrceAgora.Application.DTOs.Subscription;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Enums;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class SubscriptionService(
    ISubscriptionRepository subscriptionRepo,
    IBudgetRepository budgetRepo,
    IUserRepository userRepo,
    IAsaasService asaasService) : ISubscriptionService
{
    private const int BasicMonthlyLimit = 5;

    public async Task<SubscriptionStatusDto> GetStatusAsync(Guid userId)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var (budgets, _) = await budgetRepo.GetByUserAsync(userId, null, 1, 1000);
        var budgetsThisMonth = budgets.Count(b => b.CreatedAt >= startOfMonth);

        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);

        // Ainda é Pro se: plano pro + ativo + período não expirou
        var isPro = subscription?.Plan == "pro" &&
                    subscription?.Status == "active" &&
                    (subscription?.CurrentPeriodEnd == null ||
                     subscription.CurrentPeriodEnd >= DateOnly.FromDateTime(now));

        var canCreate = isPro || budgetsThisMonth < BasicMonthlyLimit;

        // Dias restantes se cancelamento agendado
        int? daysRemaining = null;
        if (subscription?.CancelAtPeriodEnd == true && subscription.CurrentPeriodEnd != null)
        {
            var end = subscription.CurrentPeriodEnd.Value.ToDateTime(TimeOnly.MinValue);
            daysRemaining = Math.Max(0, (int)(end - now).TotalDays);
        }

        return new SubscriptionStatusDto
        {
            Plan = isPro ? "pro" : "basic",
            Status = subscription?.Status ?? "active",
            BudgetsThisMonth = budgetsThisMonth,
            BudgetLimit = isPro ? 0 : BasicMonthlyLimit,
            CanCreateBudget = canCreate,
            RemainingBudgets = isPro ? -1 :
                Math.Max(0, BasicMonthlyLimit - budgetsThisMonth),
            CurrentPeriodEnd = subscription?.CurrentPeriodEnd,
            CancelAtPeriodEnd = subscription?.CancelAtPeriodEnd ?? false,
            DaysRemainingAfterCancel = daysRemaining
        };
    }

    public async Task<string> UpgradeToProAsync(Guid userId, UpgradeDto dto)
    {
        var user = await userRepo.GetByIdAsync(userId)
            ?? throw new Exception("Usuário não encontrado");

        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);

        var customerId = subscription?.AsaasCustomerId;
        if (customerId is null)
        {
            try
            {
                customerId = await asaasService.CreateCustomerAsync(
                    user.Name, user.Email, dto.CpfCnpj);
            }
            catch (Exception ex)
            {
                throw new Exception($"Erro ao criar cliente Asaas: {ex.Message}");
            }
        }

        string subscriptionId;
        try
        {
            subscriptionId = await asaasService
                .CreateSubscriptionAsync(customerId, "Pro");
        }
        catch (Exception ex)
        {
            throw new Exception($"Erro ao criar assinatura Asaas: {ex.Message}");
        }

        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        if (subscription is null)
        {
            subscription = new Subscription
            {
                UserId = userId,
                AsaasCustomerId = customerId,
                AsaasSubscriptionId = subscriptionId,
                Plan = "pro",
                Status = "active",
                CurrentPeriodStart = now,
                CurrentPeriodEnd = now.AddMonths(1)
            };
            await subscriptionRepo.AddAsync(subscription);
        }
        else
        {
            subscription.AsaasCustomerId = customerId;
            subscription.AsaasSubscriptionId = subscriptionId;
            subscription.Plan = "pro";
            subscription.Status = "active";
            subscription.CurrentPeriodStart = now;
            subscription.CurrentPeriodEnd = now.AddMonths(1);
            subscription.UpdatedAt = DateTime.UtcNow;
            await subscriptionRepo.UpdateAsync(subscription);
        }

        user.Plan = PlanType.Pro;
        await userRepo.UpdateAsync(user);
        await subscriptionRepo.SaveChangesAsync();

        return subscriptionId;
    }

    public async Task CancelProAsync(Guid userId)
    {
        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);
        if (subscription is null) return;

        // Cancela no Asaas mas mantém acesso até o fim do período
        if (subscription.AsaasSubscriptionId is not null)
            await asaasService.CancelSubscriptionAsync(
                subscription.AsaasSubscriptionId);

        subscription.CancelAtPeriodEnd = true;
        subscription.UpdatedAt = DateTime.UtcNow;
        // NÃO muda o plano ainda — só agenda o cancelamento
        await subscriptionRepo.UpdateAsync(subscription);
        await subscriptionRepo.SaveChangesAsync();
    }
    public async Task HandleWebhookAsync(string payload)
    {
        var doc = JsonDocument.Parse(payload);
        var root = doc.RootElement;

        var eventType = root.GetProperty("event").GetString();
        var subscriptionId = root
            .GetProperty("payment")
            .GetProperty("subscription")
            .GetString();

        if (subscriptionId is null) return;

        var subscription = await subscriptionRepo
            .GetByAsaasSubscriptionIdAsync(subscriptionId);
        if (subscription is null) return;

        subscription.Status = eventType switch
        {
            "PAYMENT_RECEIVED" => "active",
            "PAYMENT_OVERDUE" => "overdue",
            "SUBSCRIPTION_DELETED" => "cancelled",
            _ => subscription.Status
        };

        if (subscription.Status == "cancelled")
        {
            subscription.Plan = "basic";
            var user = await userRepo.GetByIdAsync(subscription.UserId);
            if (user is not null)
            {
                user.Plan = PlanType.Basic;
                await userRepo.UpdateAsync(user);
            }
        }

        subscription.UpdatedAt = DateTime.UtcNow;
        await subscriptionRepo.UpdateAsync(subscription);
        await subscriptionRepo.SaveChangesAsync();
    }
}