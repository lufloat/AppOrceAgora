using Microsoft.EntityFrameworkCore;
using OrceAgora.Domain.Entities;
using OrceAgora.Domain.Enums;

namespace OrceAgora.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ServiceTemplate> ServiceTemplates => Set<ServiceTemplate>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Budget> Budgets => Set<Budget>();

    public DbSet<EmailToken> EmailTokens => Set<EmailToken>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();
    public DbSet<LoginAttempt> LoginAttempts => Set<LoginAttempt>();
    public DbSet<BudgetItem> BudgetItems => Set<BudgetItem>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<AgendaEvent> AgendaEvents => Set<AgendaEvent>();

    protected override void OnModelCreating(ModelBuilder m)
    {
        m.Entity<User>(e => {
            e.ToTable("users");
            e.Property(u => u.Id).HasColumnName("id");
            e.Property(u => u.Name).HasColumnName("name");
            e.Property(u => u.Email).HasColumnName("email");
            e.Property(u => u.PasswordHash).HasColumnName("password_hash");
            e.Property(u => u.GoogleId).HasColumnName("google_id");
            e.Property(u => u.EmailConfirmed).HasColumnName("email_confirmed");
            e.Property(u => u.BrandColor).HasColumnName("brand_color");
            e.Property(u => u.Plan)
             .HasColumnName("plan")
             .HasConversion(
                 v => v.ToString().ToLower(),
                 v => Enum.Parse<PlanType>(v, true))
             .HasDefaultValue(PlanType.Basic);
            e.Property(u => u.LogoUrl).HasColumnName("logo_url");
            e.Property(u => u.Phone).HasColumnName("phone");
            e.Property(u => u.Address).HasColumnName("address");
            e.Property(u => u.CompanyName).HasColumnName("company_name");
            e.Property(u => u.CreatedAt).HasColumnName("created_at");
        });


        m.Entity<LoginAttempt>(e => {
            e.ToTable("login_attempts");
            e.Property(l => l.Id).HasColumnName("id");
            e.Property(l => l.Email).HasColumnName("email");
            e.Property(l => l.Ip).HasColumnName("ip");
            e.Property(l => l.AttemptedAt).HasColumnName("attempted_at");
            e.Property(l => l.Success).HasColumnName("success");
        });

        m.Entity<EmailToken>(e => {
            e.ToTable("email_tokens");
            e.Property(t => t.Id).HasColumnName("id");
            e.Property(t => t.UserId).HasColumnName("user_id");
            e.Property(t => t.Token).HasColumnName("token");
            e.Property(t => t.Type).HasColumnName("type");
            e.Property(t => t.ExpiresAt).HasColumnName("expires_at");
            e.Property(t => t.UsedAt).HasColumnName("used_at");
            e.Property(t => t.CreatedAt).HasColumnName("created_at");
        });


        m.Entity<Category>(e => {
            e.ToTable("categories");
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.UserId).HasColumnName("user_id");
            e.Property(c => c.Name).HasColumnName("name");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
        });

        m.Entity<Subscription>(e => {
            e.ToTable("subscriptions");
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.UserId).HasColumnName("user_id");
            e.Property(s => s.AsaasCustomerId).HasColumnName("asaas_customer_id");
            e.Property(s => s.AsaasSubscriptionId).HasColumnName("asaas_subscription_id");
            e.Property(s => s.Status).HasColumnName("status");
            e.Property(s => s.Plan).HasColumnName("plan");
            e.Property(s => s.CurrentPeriodStart).HasColumnName("current_period_start");
            e.Property(s => s.CurrentPeriodEnd).HasColumnName("current_period_end");
            e.Property(s => s.CreatedAt).HasColumnName("created_at");
            e.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        });

        m.Entity<ServiceTemplate>(e => {
            e.ToTable("service_templates");
            e.Property(s => s.Id).HasColumnName("id");
            e.Property(s => s.UserId).HasColumnName("user_id");
            e.Property(s => s.CategoryId).HasColumnName("category_id");
            e.Property(s => s.Name).HasColumnName("name");
            e.Property(s => s.DefaultPrice).HasColumnName("default_price");
            e.Property(s => s.Description).HasColumnName("description");
            e.Property(s => s.CreatedAt).HasColumnName("created_at");
        });

        m.Entity<Client>(e => {
            e.ToTable("clients");
            e.Property(c => c.Id).HasColumnName("id");
            e.Property(c => c.UserId).HasColumnName("user_id");
            e.Property(c => c.Name).HasColumnName("name");
            e.Property(c => c.Phone).HasColumnName("phone");
            e.Property(c => c.Email).HasColumnName("email");
            e.Property(c => c.Address).HasColumnName("address");
            e.Property(c => c.CreatedAt).HasColumnName("created_at");
        });

        m.Entity<Budget>(e => {
            e.ToTable("budgets");
            e.Property(b => b.Id).HasColumnName("id");
            e.Property(b => b.UserId).HasColumnName("user_id");
            e.Property(b => b.ClientId).HasColumnName("client_id");
            e.Property(b => b.Number).HasColumnName("number").ValueGeneratedOnAdd();
            e.Property(b => b.Status)
             .HasColumnName("status")
             .HasConversion(
                 v => v.ToString().ToLower(),
                 v => Enum.Parse<BudgetStatus>(v, true));
            e.Property(b => b.Subtotal).HasColumnName("subtotal");
            e.Property(b => b.DiscountType)
             .HasColumnName("discount_type")
             .HasConversion(
                 v => v.ToString().ToLower(),
                 v => Enum.Parse<DiscountType>(v, true));
            e.Property(b => b.DiscountValue).HasColumnName("discount_value");
            e.Property(b => b.DiscountAmount).HasColumnName("discount_amount");
            e.Property(b => b.Extras).HasColumnName("extras");
            e.Property(b => b.ExtrasDescription).HasColumnName("extras_description");
            e.Property(b => b.Total).HasColumnName("total");
            e.Property(b => b.ValidityDays).HasColumnName("validity_days");
            e.Property(b => b.Notes).HasColumnName("notes");
            e.Property(b => b.PaymentMethods).HasColumnName("payment_methods");
            e.Property(b => b.ApprovalToken).HasColumnName("approval_token");
            e.Property(b => b.ViewedAt).HasColumnName("viewed_at");
            e.Property(b => b.CreatedAt).HasColumnName("created_at");
        });

        m.Entity<BudgetItem>(e => {
            e.ToTable("budget_items");
            e.Property(i => i.Id).HasColumnName("id");
            e.Property(i => i.BudgetId).HasColumnName("budget_id");
            e.Property(i => i.TemplateId).HasColumnName("template_id");
            e.Property(i => i.Name).HasColumnName("name");
            e.Property(i => i.Qty).HasColumnName("qty");
            e.Property(i => i.UnitPrice).HasColumnName("unit_price");
            e.Property(i => i.IsCustom).HasColumnName("is_custom");
            e.Property(i => i.SortOrder).HasColumnName("sort_order");
            e.Property(i => i.IsLabor).HasColumnName("is_labor");
            e.Property(i => i.LaborType).HasColumnName("labor_type");
            e.Ignore(i => i.Total);
        });

        m.Entity<Payment>(e => {
            e.ToTable("payments");
            e.Property(p => p.Id).HasColumnName("id");
            e.Property(p => p.BudgetId).HasColumnName("budget_id");
            e.Property(p => p.Amount).HasColumnName("amount");
            e.Property(p => p.Method).HasColumnName("method");
            e.Property(p => p.Status)
             .HasColumnName("status")
             .HasConversion(
                 v => v.ToString().ToLower(),
                 v => Enum.Parse<PaymentStatus>(v, true));
            e.Property(p => p.DueDate).HasColumnName("due_date");
            e.Property(p => p.PaidAt).HasColumnName("paid_at");
            e.Property(p => p.InstallmentNumber).HasColumnName("installment_number");
            e.Property(p => p.CreatedAt).HasColumnName("created_at");
        });

        m.Entity<AgendaEvent>(e => {
            e.ToTable("agenda_events");
            e.Property(a => a.Id).HasColumnName("id");
            e.Property(a => a.UserId).HasColumnName("user_id");
            e.Property(a => a.BudgetId).HasColumnName("budget_id");
            e.Property(a => a.Title).HasColumnName("title");
            e.Property(a => a.Notes).HasColumnName("notes");
            e.Property(a => a.ReminderAt).HasColumnName("reminder_at");
            e.Property(a => a.Done).HasColumnName("done");
            e.Property(a => a.CreatedAt).HasColumnName("created_at");
        });
    }
}