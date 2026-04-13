using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.Infrastructure.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    private readonly string _apiKey =
        Environment.GetEnvironmentVariable("Resend__ApiKey")
        ?? config["Resend:ApiKey"]
        ?? throw new Exception("Resend API Key não configurada");

    private readonly string _fromEmail =
        Environment.GetEnvironmentVariable("Resend__FromEmail")
        ?? config["Resend:FromEmail"]
        ?? "noreply@orceagora.com.br";

    private async Task SendAsync(string to, string toName,
        string subject, string html)
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _apiKey);

        var body = new
        {
            from = $"OrceAgora <{_fromEmail}>",
            to = new[] { to },
            subject,
            html
        };

        var response = await client.PostAsync(
            "https://api.resend.com/emails",
            new StringContent(
                JsonSerializer.Serialize(body),
                Encoding.UTF8,
                "application/json"));

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Resend error: {error}");
        }
    }

    public Task SendConfirmationEmailAsync(
        string toEmail, string toName, string confirmUrl) =>
        SendAsync(toEmail, toName,
            "Confirme seu e-mail — OrceAgora",
            $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 480px;
                         margin: 0 auto; padding: 32px 16px; color: #1E293B;">
              <h1 style="color: #1A56DB; font-size: 24px; margin-bottom: 8px;">
                OrceAgora
              </h1>
              <p style="color: #64748B; margin-bottom: 32px;">
                Orçamentos profissionais em segundos
              </p>
              <h2 style="font-size: 20px; margin-bottom: 12px;">
                Olá, {toName}! 👋
              </h2>
              <p style="color: #475569; line-height: 1.6;">
                Obrigado por criar sua conta no OrceAgora.
                Clique no botão abaixo para confirmar seu e-mail e começar a usar.
              </p>
              <div style="text-align: center; margin: 32px 0;">
                <a href="{confirmUrl}"
                   style="background: #1A56DB; color: white; padding: 14px 32px;
                          border-radius: 8px; text-decoration: none;
                          font-weight: bold; font-size: 16px;">
                  Confirmar e-mail
                </a>
              </div>
              <p style="color: #94A3B8; font-size: 13px;">
                Este link expira em 24 horas.
                Se você não criou uma conta, ignore este e-mail.
              </p>
              <hr style="border: none; border-top: 1px solid #E2E8F0; margin: 24px 0;">
              <p style="color: #CBD5E1; font-size: 12px; text-align: center;">
                OrceAgora · orceagora.com.br
              </p>
            </body>
            </html>
            """);

    public Task SendPasswordResetEmailAsync(
        string toEmail, string toName, string resetUrl) =>
        SendAsync(toEmail, toName,
            "Redefinir senha — OrceAgora",
            $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 480px;
                         margin: 0 auto; padding: 32px 16px; color: #1E293B;">
              <h1 style="color: #1A56DB; font-size: 24px; margin-bottom: 8px;">
                OrceAgora
              </h1>
              <p style="color: #64748B; margin-bottom: 32px;">
                Orçamentos profissionais em segundos
              </p>
              <h2 style="font-size: 20px; margin-bottom: 12px;">
                Redefinir sua senha
              </h2>
              <p style="color: #475569; line-height: 1.6;">
                Olá, {toName}! Recebemos uma solicitação para redefinir
                a senha da sua conta. Clique no botão abaixo para continuar.
              </p>
              <div style="text-align: center; margin: 32px 0;">
                <a href="{resetUrl}"
                   style="background: #1A56DB; color: white; padding: 14px 32px;
                          border-radius: 8px; text-decoration: none;
                          font-weight: bold; font-size: 16px;">
                  Redefinir senha
                </a>
              </div>
              <p style="color: #94A3B8; font-size: 13px;">
                Este link expira em 1 hora.
                Se você não solicitou a redefinição, ignore este e-mail.
              </p>
              <hr style="border: none; border-top: 1px solid #E2E8F0; margin: 24px 0;">
              <p style="color: #CBD5E1; font-size: 12px; text-align: center;">
                OrceAgora · orceagora.com.br
              </p>
            </body>
            </html>
            """);
}