using OrceAgora.Application.DTOs.Subscription;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Enums;
using OrceAgora.Domain.Interfaces;
using System.Text.Json;

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

        var isPro = subscription?.Plan == "pro" &&
                    subscription?.Status == "active" &&
                    (subscription?.CurrentPeriodEnd == null ||
                     subscription.CurrentPeriodEnd >= DateOnly.FromDateTime(now));

        var canCreate = isPro || budgetsThisMonth < BasicMonthlyLimit;

        int? daysRemaining = null;
        if (subscription?.CancelAtPeriodEnd == true && subscription.CurrentPeriodEnd != null)
        {
            var end = subscription.CurrentPeriodEnd.Value.ToDateTime(TimeOnly.MinValue);
            daysRemaining = Math.Max(0, (int)(end - now).TotalDays);
        }

        return new SubscriptionStatusDto
        {
            Plan = isPro ? "pro" : "basic",
            Status = subscription?.Status ?? "inactive",
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

    public async Task<UpgradeResultDto> UpgradeToProAsync(Guid userId, UpgradeDto dto)
    {
        var user = await userRepo.GetByIdAsync(userId)
            ?? throw new Exception("Usuário não encontrado");

        if (string.IsNullOrWhiteSpace(dto.CpfCnpj))
            throw new Exception("CPF/CNPJ é obrigatório para assinar o plano Pro.");

        if (!IsValidCpfCnpj(dto.CpfCnpj))
            throw new Exception("CPF/CNPJ inválido. Verifique e tente novamente.");

        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);

        var customerId = subscription?.AsaasCustomerId
            ?? await asaasService.CreateCustomerAsync(user.Name, user.Email, dto.CpfCnpj);

        var result = await asaasService.CreateSubscriptionAsync(customerId, "Pro");

        var now = DateOnly.FromDateTime(DateTime.UtcNow);

        if (subscription is null)
        {
            subscription = new Subscription
            {
                UserId = userId,
                AsaasCustomerId = customerId,
                AsaasSubscriptionId = result.SubscriptionId,
                Plan = "pro",
                Status = "inactive", // aguarda confirmação do pagamento via webhook
                CurrentPeriodStart = now,
                CurrentPeriodEnd = now.AddMonths(1)
            };
            await subscriptionRepo.AddAsync(subscription);
        }
        else
        {
            subscription.AsaasCustomerId = customerId;
            subscription.AsaasSubscriptionId = result.SubscriptionId;
            subscription.Plan = "pro";
            subscription.Status = "inactive"; // aguarda confirmação do pagamento via webhook
            subscription.CancelAtPeriodEnd = false;
            subscription.CurrentPeriodStart = now;
            subscription.CurrentPeriodEnd = now.AddMonths(1);
            subscription.UpdatedAt = DateTime.UtcNow;
            await subscriptionRepo.UpdateAsync(subscription);
        }

        // NÃO libera o Pro aqui — só via webhook PAYMENT_RECEIVED
        await subscriptionRepo.SaveChangesAsync();

        return new UpgradeResultDto
        {
            SubscriptionId = result.SubscriptionId,
            PaymentUrl = result.PaymentUrl,
            PixCode = result.PixCode,
            PixQrCodeUrl = result.PixQrCodeUrl,
            Message = "Assinatura criada! Realize o pagamento via Pix para ativar o plano Pro."
        };
    }

    public async Task CancelProAsync(Guid userId)
    {
        var subscription = await subscriptionRepo.GetByUserIdAsync(userId);
        if (subscription is null) return;

        if (subscription.AsaasSubscriptionId is not null)
            await asaasService.CancelSubscriptionAsync(subscription.AsaasSubscriptionId);

        subscription.CancelAtPeriodEnd = true;
        subscription.UpdatedAt = DateTime.UtcNow;
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

        // Pagamento confirmado — libera o Pro
        if (eventType == "PAYMENT_RECEIVED")
        {
            subscription.Plan = "pro";
            var user = await userRepo.GetByIdAsync(subscription.UserId);
            if (user is not null)
            {
                user.Plan = PlanType.Pro;
                await userRepo.UpdateAsync(user);
            }
        }

        // Assinatura cancelada — rebaixa para Basic
        if (eventType == "SUBSCRIPTION_DELETED")
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

    // ── Validação CPF/CNPJ ──────────────────────────────────────────

    private static bool IsValidCpfCnpj(string value)
    {
        var digits = new string(value.Where(char.IsDigit).ToArray());
        return digits.Length == 11 ? IsValidCpf(digits)
             : digits.Length == 14 ? IsValidCnpj(digits)
             : false;
    }

    private static bool IsValidCpf(string cpf)
    {
        if (cpf.Distinct().Count() == 1) return false;

        var sum = 0;
        for (int i = 0; i < 9; i++)
            sum += int.Parse(cpf[i].ToString()) * (10 - i);
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;
        if (digit1 != int.Parse(cpf[9].ToString())) return false;

        sum = 0;
        for (int i = 0; i < 10; i++)
            sum += int.Parse(cpf[i].ToString()) * (11 - i);
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;
        return digit2 == int.Parse(cpf[10].ToString());
    }

    private static bool IsValidCnpj(string cnpj)
    {
        if (cnpj.Distinct().Count() == 1) return false;

        int[] weights1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] weights2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        var sum = 0;
        for (int i = 0; i < 12; i++)
            sum += int.Parse(cnpj[i].ToString()) * weights1[i];
        var remainder = sum % 11;
        var digit1 = remainder < 2 ? 0 : 11 - remainder;
        if (digit1 != int.Parse(cnpj[12].ToString())) return false;

        sum = 0;
        for (int i = 0; i < 13; i++)
            sum += int.Parse(cnpj[i].ToString()) * weights2[i];
        remainder = sum % 11;
        var digit2 = remainder < 2 ? 0 : 11 - remainder;
        return digit2 == int.Parse(cnpj[13].ToString());
    }
}