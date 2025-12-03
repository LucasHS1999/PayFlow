using System.Net.Http.Json;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddSingleton<IPaymentFeeService, PaymentFeeService>();

var app = builder.Build();

// Pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Minimal API endpoint - Payments
app.MapPost("/payments", async (
    PaymentRequest request,
    IHttpClientFactory httpClientFactory,
    IConfiguration config,
    IPaymentFeeService feeService,
    ILogger<Program> logger) =>
{
    if (request.amount <= 0 || string.IsNullOrWhiteSpace(request.currency))
        return Results.BadRequest(new { error = "Invalid amount or currency" });

    var provider = request.amount < 100m ? "FastPay" : "SecurePay";
    var fastPayUrl = config["FASTPAY_URL"];
    var securePayUrl = config["SECUREPAY_URL"];

    var client = httpClientFactory.CreateClient();
    HttpResponseMessage providerResponse;
    string externalId;
    string status;

    try
    {
        if (provider == "FastPay")
        {
            providerResponse = await client.PostAsJsonAsync(fastPayUrl, request);
            providerResponse.EnsureSuccessStatusCode();
            var fp = await providerResponse.Content.ReadFromJsonAsync<FastPayResponse>();
            externalId = fp?.id ?? string.Empty;
            status = fp?.status ?? "unknown";
        }
        else
        {
            providerResponse = await client.PostAsJsonAsync(securePayUrl, request);
            providerResponse.EnsureSuccessStatusCode();
            var sp = await providerResponse.Content.ReadFromJsonAsync<SecurePayResponse>();
            externalId = sp?.transaction_id ?? string.Empty;
            status = sp?.result ?? "unknown";
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Provider call failed");
        return Results.Problem(title: "Provider call failed", statusCode: StatusCodes.Status502BadGateway);
    }

    var fee = feeService.CalculateFee(request.amount, request.currency);
    var net = request.amount - fee;

    var canonical = new CanonicalResponse(externalId, provider, status, request.amount, fee, net);
    return Results.Ok(canonical);
})
.WithName("CreatePayment")
.WithTags("Payments")
.Produces<CanonicalResponse>(StatusCodes.Status200OK)
.Produces(StatusCodes.Status400BadRequest)
.Produces(StatusCodes.Status502BadGateway)
.AddOpenApiOperationTransformer((operation, context, ct) =>
{
    operation.Summary = "Create a payment";
    operation.Description = "Business rules: if amount < 100 the provider used is FastPay, otherwise SecurePay. Fees: 1.5% for amounts below 100, else 2.5%. The provider URLs are read from configuration keys FASTPAY_URL and SECUREPAY_URL.";
    return Task.CompletedTask;
});

// Health
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "PayFlow.Api" }));

await app.RunAsync();

public interface IPaymentFeeService
{
    decimal CalculateFee(decimal amount, string currency);
}

public class PaymentFeeService : IPaymentFeeService
{
    public decimal CalculateFee(decimal amount, string currency)
    {
        // Simple fee rule: 1.5% up to 100, else 2.5%
        var rate = amount < 100m ? 0.015m : 0.025m;
        return decimal.Round(amount * rate, 2);
    }
}

public record PaymentRequest(decimal amount, string currency);
public record CanonicalResponse(string externalId, string provider, string status, decimal grossAmount, decimal fee, decimal netAmount);

internal record FastPayResponse(string id, string status, string status_detail);
internal record SecurePayResponse(string transaction_id, string result);
