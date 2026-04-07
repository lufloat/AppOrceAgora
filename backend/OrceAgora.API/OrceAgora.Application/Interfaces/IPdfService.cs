using OrceAgora.Application.DTOs.Budgets;
using OrceAgora.Domain.Entities;

namespace OrceAgora.Application.Interfaces;

public interface IPdfService
{
    byte[] GenerateBudgetPdf(BudgetDto budget, User professional);
}