using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrceAgora.Application.Interfaces;
using OrceAgora.Application.Services;
using OrceAgora.Domain.Interfaces;
using OrceAgora.Infrastructure.Data;
using OrceAgora.Infrastructure.Repositories;
using OrceAgora.Infrastructure.Services;

namespace OrceAgora.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Banco — só AddDbContext, nunca AddDbContextFactory junto
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        // Repositórios
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IClientRepository, ClientRepository>();
        services.AddScoped<IServiceTemplateRepository, ServiceTemplateRepository>();
        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<IDashboardRepository, DashboardRepository>();
        services.AddScoped<ILoginAttemptRepository, LoginAttemptRepository>();
        services.AddScoped<IAgendaRepository, AgendaRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IEmailTokenRepository, EmailTokenRepository>();
       


        // Serviços de infraestrutura
        services.AddScoped<ITokenService, TokenService>();
        services.AddScoped<IPdfService, PdfService>();

        // Serviços de aplicação
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IClientService, ClientService>();
        services.AddScoped<IServiceTemplateService, ServiceTemplateService>();
        services.AddScoped<IBudgetService, BudgetService>();
        services.AddScoped<IDashboardService, DashboardService>();
        services.AddScoped<IProfileService, ProfileService>();
        services.AddScoped<IAgendaService, AgendaService>();
        services.AddScoped<IAsaasService, AsaasService>();
        services.AddScoped<ISubscriptionService, SubscriptionService>();
        services.AddScoped<IEmailService, EmailService>();


        return services;
    }
}