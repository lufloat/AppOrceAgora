using OrceAgora.Application.DTOs.Budgets;
using OrceAgora.Application.DTOs.Clients;
using OrceAgora.Application.Interfaces;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Enums;
using OrceAgora.Domain.Interfaces;

namespace OrceAgora.Application.Services;

public class BudgetService(
    IBudgetRepository budgetRepo,
    IClientRepository clientRepo,
    IUserRepository userRepo,
    IPdfService pdfService,
    ISubscriptionService subscriptionService) : IBudgetService
{
    public async Task<PaginatedBudgetsDto> GetAllAsync(
        Guid userId, string? status, int page, int pageSize)
    {
        var (items, total) = await budgetRepo.GetByUserAsync(userId, status, page, pageSize);
        return new PaginatedBudgetsDto
        {
            Items = items.Select(MapToList).ToList(),
            Total = total,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<BudgetDto?> GetByIdAsync(Guid id, Guid userId)
    {
        var budget = await budgetRepo.GetByIdAsync(id, userId);
        return budget is null ? null : MapToDto(budget);
    }

    public async Task<BudgetDto?> GetByApprovalTokenAsync(Guid token)
    {
        var budget = await budgetRepo.GetByApprovalTokenAsync(token);
        if (budget is null) return null;

        if (budget.ViewedAt is null && budget.Status == BudgetStatus.Sent)
        {
            budget.ViewedAt = DateTime.UtcNow;
            budget.Status = BudgetStatus.Viewed;
            await budgetRepo.UpdateAsync(budget);
            await budgetRepo.SaveChangesAsync();
        }

        return MapToDto(budget);
    }

    public async Task<BudgetDto> CreateAsync(Guid userId, CreateBudgetDto dto)
    {
        // Verifica limite do plano básico
        var status = await subscriptionService.GetStatusAsync(userId);
        if (!status.CanCreateBudget)
            throw new InvalidOperationException(
                $"Limite de {status.BudgetLimit} orçamentos por mês atingido. " +
                $"Faça upgrade para o plano Pro.");

        Guid? clientId = dto.ClientId;
        if (clientId is null && !string.IsNullOrWhiteSpace(dto.ClientName))
        {
            var newClient = new Client
            {
                UserId = userId,
                Name = dto.ClientName,
                Phone = dto.ClientPhone
            };
            await clientRepo.AddAsync(newClient);
            await clientRepo.SaveChangesAsync();
            clientId = newClient.Id;
        }

        var budget = new Budget
        {
            UserId = userId,
            ClientId = clientId,
            Status = BudgetStatus.Draft,
            DiscountType = dto.DiscountType == "percent"
                ? DiscountType.Percent : DiscountType.Fixed,
            DiscountValue = dto.DiscountValue,
            Extras = dto.Extras,
            ExtrasDescription = dto.ExtrasDescription,
            ValidityDays = dto.ValidityDays,
            Notes = dto.Notes,
            PaymentMethods = dto.PaymentMethods,
            Items = dto.Items.Select((item, index) => new BudgetItem
            {
                Name = item.Name,
                Qty = item.Qty,
                UnitPrice = item.UnitPrice,
                TemplateId = item.TemplateId,
                IsCustom = item.TemplateId is null,
                IsLabor = item.IsLabor,
                LaborType = item.LaborType,
                SortOrder = index
            }).ToList()
        };

        budget.RecalculateTotals();

        await budgetRepo.AddAsync(budget);
        await budgetRepo.SaveChangesAsync();
        return MapToDto(budget);
    }

    public async Task<bool> DeleteAsync(Guid id, Guid userId)
    {
        var budget = await budgetRepo.GetByIdAsync(id, userId);
        if (budget is null) return false;
        await budgetRepo.DeleteAsync(budget);
        await budgetRepo.SaveChangesAsync();
        return true;
    }

    public async Task<BudgetDto?> ProcessApprovalAsync(Guid token, ApproveRejectDto dto)
    {
        var budget = await budgetRepo.GetByApprovalTokenAsync(token);
        if (budget is null) return null;

        budget.Status = dto.Action == "approve"
            ? BudgetStatus.Approved
            : BudgetStatus.Rejected;

        await budgetRepo.UpdateAsync(budget);
        await budgetRepo.SaveChangesAsync();
        return MapToDto(budget);
    }

    public async Task<(BudgetDto Budget, byte[] Pdf)?> GeneratePdfAsync(Guid id, Guid userId)
    {
        var budget = await budgetRepo.GetByIdAsync(id, userId);
        if (budget is null) return null;

        var user = await userRepo.GetByIdAsync(userId);
        if (user is null) return null;

        var dto = MapToDto(budget);
        var pdf = pdfService.GenerateBudgetPdf(dto, user);

        return (dto, pdf);
    }

    // --- Mappers ---

    private static BudgetDto MapToDto(Budget b) => new()
    {
        Id = b.Id,
        Number = b.Number,
        Status = b.Status.ToString().ToLower(),
        Client = b.Client is null ? null : new ClientDto
        {
            Id = b.Client.Id,
            Name = b.Client.Name,
            Phone = b.Client.Phone,
            Email = b.Client.Email,
            Address = b.Client.Address
        },
        Items = b.Items.OrderBy(i => i.SortOrder).Select(i => new BudgetItemDto
        {
            Id = i.Id,
            Name = i.Name,
            Qty = i.Qty,
            UnitPrice = i.UnitPrice,
            Total = i.Qty * i.UnitPrice,
            TemplateId = i.TemplateId,
            IsCustom = i.IsCustom,
            IsLabor = i.IsLabor,
            LaborType = i.LaborType
        }).ToList(),
        Subtotal = b.Subtotal,
        DiscountType = b.DiscountType.ToString().ToLower(),
        DiscountValue = b.DiscountValue,
        DiscountAmount = b.DiscountAmount,
        Extras = b.Extras,
        ExtrasDescription = b.ExtrasDescription,
        Total = b.Total,
        ValidityDays = b.ValidityDays,
        Notes = b.Notes,
        PaymentMethods = b.PaymentMethods,
        ApprovalToken = b.ApprovalToken,
        ApprovalLink = $"https://orceagora.com.br/aprovar/{b.ApprovalToken}",
        ViewedAt = b.ViewedAt,
        CreatedAt = b.CreatedAt,
        ExpiresAt = b.CreatedAt.AddDays(b.ValidityDays)
    };

    private static BudgetListDto MapToList(Budget b) => new()
    {
        Id = b.Id,
        Number = b.Number,
        Status = b.Status.ToString().ToLower(),
        ClientName = b.Client?.Name,
        Total = b.Total,
        CreatedAt = b.CreatedAt,
        ExpiresAt = b.CreatedAt.AddDays(b.ValidityDays)
    };
}