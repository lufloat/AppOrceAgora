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

        var (budgets, _) = await budgetRepo.GetByUserAsync(
            userId, null, 1, 1000);

        var budgetsThisMonth = budgets
            .Count(b => b.CreatedAt >= startOfMonth);

        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);
        var isPro = subscription?.Plan == "pro" &&
                    subscription?.Status == "active";

        var limit = isPro ? int.MaxValue : BasicMonthlyLimit;
        var canCreate = isPro || budgetsThisMonth < BasicMonthlyLimit;

        return new SubscriptionStatusDto
        {
            Plan = isPro ? "pro" : "basic",
            Status = subscription?.Status ?? "active",
            BudgetsThisMonth = budgetsThisMonth,
            BudgetLimit = isPro ? 0 : BasicMonthlyLimit,
            CanCreateBudget = canCreate,
            RemainingBudgets = isPro ? -1 :
                Math.Max(0, BasicMonthlyLimit - budgetsThisMonth),
            CurrentPeriodEnd = subscription?.CurrentPeriodEnd
        };
    }

    public async Task<string> UpgradeToProAsync(Guid userId, UpgradeDto dto)
    {
        var user = await userRepo.GetByIdAsync(userId)
            ?? throw new Exception("Usuário não encontrado");

        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);

        // Cria cliente no Asaas se não existir
        var customerId = subscription?.AsaasCustomerId;
        if (customerId is null)
        {
            customerId = await asaasService.CreateCustomerAsync(
                user.Name, user.Email, dto.CpfCnpj);
        }

        // Cria assinatura
        var subscriptionId = await asaasService
            .CreateSubscriptionAsync(customerId, "Pro");

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

        // Atualiza o plano do usuário
        user.Plan = PlanType.Pro;
        await userRepo.UpdateAsync(user);
        await subscriptionRepo.SaveChangesAsync();

        return subscriptionId;
    }

    public async Task CancelProAsync(Guid userId)
    {
        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);
        if (subscription?.AsaasSubscriptionId is null) return;

        await asaasService.CancelSubscriptionAsync(
            subscription.AsaasSubscriptionId);

        subscription.Status = "cancelled";
        subscription.Plan = "basic";
        subscription.UpdatedAt = DateTime.UtcNow;
        await subscriptionRepo.UpdateAsync(subscription);

        var user = await userRepo.GetByIdAsync(userId);
        if (user is not null)
        {
            user.Plan = PlanType.Basic;
            await userRepo.UpdateAsync(user);
        }

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