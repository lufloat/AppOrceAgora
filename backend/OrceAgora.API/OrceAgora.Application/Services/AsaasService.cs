using Microsoft.Extensions.Configuration;
using OrceAgora.Application.Interfaces;
using System.Text;
using System.Text.Json;
using static OrceAgora.Application.DTOs.Subscription.SubscriptionStatusDto;

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

    public async Task<AsaasSubscriptionResult> CreateSubscriptionAsync(
    string customerId, string planName)
    {
        using var client = CreateClient();

        var body = new
        {
            customer = customerId,
            billingType = "PIX",
            value = 29.90,
            nextDueDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            cycle = "MONTHLY",
            description = $"StimServ {planName}",
            externalReference = customerId
        };

        var response = await client.PostAsync(
            $"{_baseUrl}/subscriptions",
            new StringContent(JsonSerializer.Serialize(body),
                Encoding.UTF8, "application/json"));

        var json = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Asaas error: {json}");

        var doc = JsonDocument.Parse(json);
        var subscriptionId = doc.RootElement
            .GetProperty("id").GetString()!;

        // Busca o primeiro pagamento gerado para pegar o Pix
        var paymentResponse = await client.GetAsync(
            $"{_baseUrl}/payments?subscription={subscriptionId}");

        var paymentJson = await paymentResponse.Content.ReadAsStringAsync();
        var paymentDoc = JsonDocument.Parse(paymentJson);

        string? paymentUrl = null;
        string? pixCode = null;
        string? pixQrCodeUrl = null;

        try
        {
            var payments = paymentDoc.RootElement.GetProperty("data");
            if (payments.GetArrayLength() > 0)
            {
                var firstPayment = payments[0];
                var paymentId = firstPayment.GetProperty("id").GetString();

                // Busca o QR Code Pix
                var pixResponse = await client.GetAsync(
                    $"{_baseUrl}/payments/{paymentId}/pixQrCode");
                var pixJson = await pixResponse.Content.ReadAsStringAsync();
                var pixDoc = JsonDocument.Parse(pixJson);

                pixDoc.RootElement.TryGetProperty("payload", out var payloadProp);
                pixDoc.RootElement.TryGetProperty("encodedImage", out var imageProp);

                pixCode = payloadProp.GetString();
                pixQrCodeUrl = imageProp.GetString();
                paymentUrl = $"https://www.asaas.com/i/{paymentId}";
            }
        }
        catch { /* ignora erro ao buscar Pix */ }

        return new AsaasSubscriptionResult
        {
            SubscriptionId = subscriptionId,
            PaymentUrl = paymentUrl,
            PixCode = pixCode,
            PixQrCodeUrl = pixQrCodeUrl
        };
    }

    public async Task CancelSubscriptionAsync(string subscriptionId)
    {
        using var client = CreateClient();
        await client.DeleteAsync($"{_baseUrl}/subscriptions/{subscriptionId}");
    }
}