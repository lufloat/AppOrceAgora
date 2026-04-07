using OrceAgora.Application.DTOs.Dashboard;

namespace OrceAgora.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid userId);
}