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
    client.DefaultRequestHeaders.Add("User-Agent", "OrceAgoraApp/1.0");

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

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Asaas CreateCustomer error: {json}");

        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("id", out var customerId))
            throw new Exception($"Asaas CreateCustomer: id não encontrado. Resposta: {json}");

        return customerId.GetString()!;
    }

    public async Task<string> CreateSubscriptionAsync(
        string customerId, string planName)
    {
        using var client = CreateClient();

        var body = new
        {
            customer = customerId,
            billingType = "UNDEFINED",
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

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Asaas CreateSubscription error: {json}");

        var doc = JsonDocument.Parse(json);

        if (!doc.RootElement.TryGetProperty("id", out var subscriptionId))
            throw new Exception($"Asaas CreateSubscription: id não encontrado. Resposta: {json}");

        return subscriptionId.GetString()!;
    }

    public async Task CancelSubscriptionAsync(string subscriptionId)
    {
        using var client = CreateClient();
        await client.DeleteAsync($"{_baseUrl}/subscriptions/{subscriptionId}");
    }
}