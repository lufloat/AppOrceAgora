using System.ComponentModel.DataAnnotations;
using OrceAgora.Application.DTOs.Clients;

namespace OrceAgora.Application.DTOs.Budgets;

public record CreateBudgetItemDto(
    string Name,
    decimal Qty,
    decimal UnitPrice,
    Guid? TemplateId,
    bool IsLabor = false,
    string? LaborType = null  // "hour" | "day" | "total"
);

public record CreateBudgetDto(
    Guid? ClientId,
    string? ClientName,
    string? ClientPhone,
    [Required] List<CreateBudgetItemDto> Items,
    string DiscountType,
    decimal DiscountValue,
    decimal Extras,
    string? ExtrasDescription,
    int ValidityDays,
    string? Notes,
    string? PaymentMethods
);

public record UpdateBudgetStatusDto([Required] string Status);

public record ApproveRejectDto(
    [Required] string Action, // "approve" | "reject"
    string? Reason
);

public class BudgetItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public Guid? TemplateId { get; set; }
    public bool IsCustom { get; set; }
    public bool IsLabor { get; set; }
    public string? LaborType { get; set; }
}

public class BudgetDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Status { get; set; } = string.Empty;
    public ClientDto? Client { get; set; }
    public List<BudgetItemDto> Items { get; set; } = [];
    public decimal Subtotal { get; set; }
    public string DiscountType { get; set; } = string.Empty;
    public decimal DiscountValue { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal Extras { get; set; }
    public string? ExtrasDescription { get; set; }
    public decimal Total { get; set; }
    public int ValidityDays { get; set; }
    public string? Notes { get; set; }
    public string? PaymentMethods { get; set; }
    public Guid ApprovalToken { get; set; }
    public string ApprovalLink { get; set; } = string.Empty;
    public DateTime? ViewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class BudgetListDto
{
    public Guid Id { get; set; }
    public int Number { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ClientName { get; set; }
    public decimal Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

public class PaginatedBudgetsDto
{
    public List<BudgetListDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}