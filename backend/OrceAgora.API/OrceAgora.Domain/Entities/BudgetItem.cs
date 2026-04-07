namespace OrceAgora.Domain.Entities;

public class BudgetItem
{
    public Guid Id { get; set; }
    public Guid BudgetId { get; set; }
    public Guid? TemplateId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Qty { get; set; } = 1;
    public decimal UnitPrice { get; set; }
    public decimal Total => Qty * UnitPrice;
    public bool IsCustom { get; set; }
    public int SortOrder { get; set; }

    public Budget Budget { get; set; } = null!;
    public ServiceTemplate? Template { get; set; }

    public bool IsLabor { get; set; }
    public string? LaborType { get; set; }
}