using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using OrceAgora.Application.Interfaces;

namespace OrceAgora.Infrastructure.Services;

public class AsaasService(IConfiguration config) : IAsaasService
{
    private readonly string _apiKey =
        Environment.GetEnvironmentVariable("Asaas__ApiKey")
        ?? config["Asaas:ApiKey"]
        ?? throw new Exception("Asaas API Key não configurada");

    private readonly string _baseUrl =
        Environment.GetEnvironmentVariable("Asaas__BaseUrl")
        ?? config["Asaas:BaseUrl"]
        ?? "https://sandbox.asaas.com/api/v3";

    private HttpClient CreateClient()
    {
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add("access_token", _apiKey);
        return client;
    }

    public async Task<string> CreateCustomerAsync(
        string name, string email, string? cpfCnpj)
    {
        using var client = CreateClient();

        var body = new
        {
            name,
            email,
            cpfCnpj = cpfCnpj ?? "00000000000",
            notificationDisabled = false
        };

        var response = await client.PostAsync(
            $"{_baseUrl}/customers",
            new StringContent(JsonSerializer.Serialize(body),
                Encoding.UTF8, "application/json"));

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }

    public async Task<string> CreateSubscriptionAsync(
        string customerId, string planName)
    {
        using var client = CreateClient();

        var body = new
        {
            customer = customerId,
            billingType = "UNDEFINED", // Pix ou cartão
            value = 29.90,
            nextDueDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            cycle = "MONTHLY",
            description = $"OrceAgora {planName}",
            externalReference = customerId
        };

        var response = await client.PostAsync(
            $"{_baseUrl}/subscriptions",
            new StringContent(JsonSerializer.Serialize(body),
                Encoding.UTF8, "application/json"));

        var json = await response.Content.ReadAsStringAsync();
        var doc = JsonDocument.Parse(json);
        return doc.RootElement.GetProperty("id").GetString()!;
    }

    public async Task CancelSubscriptionAsync(string subscriptionId)
    {
        using var client = CreateClient();
        await client.DeleteAsync($"{_baseUrl}/subscriptions/{subscriptionId}");
    }
}