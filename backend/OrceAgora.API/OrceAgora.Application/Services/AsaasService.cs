using Microsoft.Extensions.Configuration;
using OrceAgora.Application.Interfaces;
using System.Text;
using System.Text.Json;

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

    public async Task<string> CreateCustomerAsync(string name, string email, string? cpfCnpj)
    {
        if (string.IsNullOrWhiteSpace(cpfCnpj))
            throw new Exception("CPF/CNPJ é obrigatório para criar assinatura.");

        using var client = CreateClient();

        var body = new
        {
            name,
            email,
            cpfCnpj,
            notificationDisabled = false
        };

        var response = await client.PostAsync(
            $"{_baseUrl}/customers",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var json = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"[ASAAS CreateCustomer] {response.StatusCode} | {json}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Asaas CreateCustomer error: {json}");

        var doc = JsonDocument.Parse(json);

        // Asaas pode retornar cliente já existente com mesmo CPF
        if (doc.RootElement.TryGetProperty("id", out var customerId))
            return customerId.GetString()!;

        throw new Exception($"Asaas CreateCustomer: id não encontrado. Resposta: {json}");
    }

    public async Task<AsaasSubscriptionResult> CreateSubscriptionAsync(string customerId, string planName)
    {
        using var client = CreateClient();

        var body = new
        {
            customer = customerId,
            billingType = "PIX",
            value = 29.90,
            nextDueDate = DateTime.UtcNow.AddDays(1).ToString("yyyy-MM-dd"),
            cycle = "MONTHLY",
            description = $"OrceAgora {planName}",
            externalReference = customerId
        };

        var response = await client.PostAsync(
            $"{_baseUrl}/subscriptions",
            new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

        var json = await response.Content.ReadAsStringAsync();

        Console.WriteLine($"[ASAAS CreateSubscription] {response.StatusCode} | {json}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Asaas CreateSubscription error: {json}");

        var doc = JsonDocument.Parse(json);
        var subscriptionId = doc.RootElement.GetProperty("id").GetString()!;

        var paymentResponse = await client.GetAsync(
            $"{_baseUrl}/payments?subscription={subscriptionId}");

        var paymentJson = await paymentResponse.Content.ReadAsStringAsync();

        Console.WriteLine($"[ASAAS GetPayments] {paymentResponse.StatusCode} | {paymentJson}");

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

                var pixResponse = await client.GetAsync(
                    $"{_baseUrl}/payments/{paymentId}/pixQrCode");
                var pixJson = await pixResponse.Content.ReadAsStringAsync();

                Console.WriteLine($"[ASAAS GetPixQrCode] {pixResponse.StatusCode} | {pixJson}");

                var pixDoc = JsonDocument.Parse(pixJson);

                pixDoc.RootElement.TryGetProperty("payload", out var payloadProp);
                pixDoc.RootElement.TryGetProperty("encodedImage", out var imageProp);

                pixCode = payloadProp.ValueKind != JsonValueKind.Undefined
                    ? payloadProp.GetString() : null;

                pixQrCodeUrl = imageProp.ValueKind != JsonValueKind.Undefined
                    ? imageProp.GetString() : null;

                paymentUrl = $"https://www.asaas.com/i/{paymentId}";
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ASAAS Pix ERROR] {ex.Message}");
        }

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
        var response = await client.DeleteAsync(
            $"{_baseUrl}/subscriptions/{subscriptionId}");

        Console.WriteLine($"[ASAAS CancelSubscription] {response.StatusCode}");
    }
}