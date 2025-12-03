using System.Net.Http.Json;
using PayFlow.Api.Contracts.Requests;
using PayFlow.Api.Contracts.Responses;
using PayFlow.Api.Domain.Interfaces;
using PayFlow.Api.Domain.Services;

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

    var primaryProvider = request.amount < 100m ? "FastPay" : "SecurePay";
    var secondaryProvider = primaryProvider == "FastPay" ? "SecurePay" : "FastPay";

    var fastPayUrl = config["FASTPAY_URL"];
    var securePayUrl = config["SECUREPAY_URL"];

    if (string.IsNullOrWhiteSpace(fastPayUrl) || string.IsNullOrWhiteSpace(securePayUrl))
        return Results.Problem(title: "Provider configuration missing", statusCode: StatusCodes.Status500InternalServerError);

    var client = httpClientFactory.CreateClient();

    async Task<(bool ok, string externalId, string status)> TryCallAsync(string providerName)
    {
        try
        {
            if (providerName == "FastPay")
            {
                var resp = await client.PostAsJsonAsync(fastPayUrl, request);
                resp.EnsureSuccessStatusCode();
                var fp = await resp.Content.ReadFromJsonAsync<FastPayResponse>();
                return (true, fp?.id ?? string.Empty, fp?.status ?? "unknown");
            }
            else
            {
                var resp = await client.PostAsJsonAsync(securePayUrl, request);
                resp.EnsureSuccessStatusCode();
                var sp = await resp.Content.ReadFromJsonAsync<SecurePayResponse>();
                return (true, sp?.transaction_id ?? string.Empty, sp?.result ?? "unknown");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "{Provider} call failed, will try fallback provider", providerName);
            return (false, string.Empty, "unknown");
        }
    }

    var primaryResult = await TryCallAsync(primaryProvider);
    string providerUsed;
    string externalId;
    string status;

    if (primaryResult.ok)
    {
        providerUsed = primaryProvider;
        externalId = primaryResult.externalId;
        status = primaryResult.status;
    }
    else
    {
        var secondaryResult = await TryCallAsync(secondaryProvider);
        if (!secondaryResult.ok)
        {
            logger.LogError("Both providers failed: {Primary} and {Secondary}", primaryProvider, secondaryProvider);
            return Results.Problem(title: "Provider call failed", statusCode: StatusCodes.Status502BadGateway);
        }

        providerUsed = secondaryProvider;
        externalId = secondaryResult.externalId;
        status = secondaryResult.status;
    }

    var fee = feeService.CalculateFee(request.amount, request.currency);
    var net = request.amount - fee;

    var canonical = new CanonicalResponse(externalId, providerUsed, status, request.amount, fee, net);
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
    operation.Description = "Business rules: if amount < 100 the provider used is FastPay, otherwise SecurePay. If the chosen provider is unavailable, the other provider is attempted regardless of the amount rule. Fees: 1.5% for amounts below 100, else 2.5%. The provider URLs are read from configuration keys FASTPAY_URL and SECUREPAY_URL.";

    return Task.CompletedTask;
});

// Health
app.MapGet("/", () => Results.Ok(new { status = "ok", service = "PayFlow.Api" }));

await app.RunAsync();
