// book/samples/Chapter-01/DistributedPaymentService/src/PaymentService.Api/Program.cs
//
// Composition root — the single place that assembles all dependencies.
// Architecture note (S03): the only file that knows about all layers.
// Every other file knows only its own layer and the layers it depends on.

using PaymentService.Api.Endpoints;
using PaymentService.Api.Serialization;
using PaymentService.Api.Workers;
using PaymentService.Application.Ports;
using PaymentService.Application.UseCases;
using PaymentService.Infrastructure.Gateway;
using PaymentService.Infrastructure.Messaging;
using PaymentService.Infrastructure.Processing;
using PaymentService.Infrastructure.Repositories;
using PaymentService.Infrastructure.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// ── HTTP JSON: translate boundary strings to domain value objects ─────────
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new CustomerIdJsonConverter());
    options.SerializerOptions.Converters.Add(new PaymentIdJsonConverter());
});

// ── Infrastructure ────────────────────────────────────────────────────────

// Redis distributed cache for preferences endpoint (S03 concept).
// No Redis running locally? The endpoint degrades to source-of-truth reads
// (see PreferencesEndpoint) — the app still starts and serves traffic.
//
// Budget honesty: StackExchange.Redis handshakes ignore cancellation tokens
// and wait out their own ConnectTimeout (default 5 s) — ten times the
// endpoint's 500 ms budget. These options fail the handshake fast so the
// documented 50 ms cache bound holds even when Redis is absent.
builder.Services.AddStackExchangeRedisCache(options =>
{
    var configuration = builder.Configuration.GetConnectionString("Redis")
        ?? "localhost:6379";
    var redisOptions = ConfigurationOptions.Parse(configuration);
    redisOptions.ConnectTimeout = 50;
    redisOptions.SyncTimeout = 50;
    redisOptions.AbortOnConnectFail = true;
    options.ConfigurationOptions = redisOptions;
});

// In-memory adapters are singletons DELIBERATELY: their dictionaries are the
// stand-in durable store. Scoped instances would lose idempotency keys and
// outbox events between requests — the exact failure S01 documents.
// Production swaps these registrations for Redis/EF Core/Postgres adapters
// behind the same Ports interfaces; no consumer changes.
builder.Services.AddSingleton<IPaymentRepository, InMemoryPaymentRepository>();
builder.Services.AddSingleton<InMemoryMessageBus>();
builder.Services.AddSingleton<IMessageBus>(
    static sp => sp.GetRequiredService<InMemoryMessageBus>());
builder.Services.AddSingleton<IDataProcessor, LoggingDataProcessor>();
builder.Services.AddSingleton<IUserPreferenceService, InMemoryUserPreferenceService>();

// Gateway: stub by default so the sample runs with zero external
// dependencies. Set ExternalGateway:UseStub=false plus a real
// ExternalGateway:BaseUrl for the Polly-pipelined production client.
// Both implement Ports.IPaymentGateway; consumers cannot tell them apart.
if (builder.Configuration.GetValue("ExternalGateway:UseStub", true))
{
    builder.Services.AddSingleton<IPaymentGateway, StubGatewayClient>();
}
else
{
    builder.Services.AddExternalGatewayClient()
        .ConfigureHttpClient(client =>
        {
            client.BaseAddress = new Uri(
                builder.Configuration["ExternalGateway:BaseUrl"]
                    ?? "https://api.payment-provider.example.com");
        });
}

// ── Application ───────────────────────────────────────────────────────────

builder.Services.AddScoped<PaymentProcessor>();

// ── Workers ───────────────────────────────────────────────────────────────

// DataProcessingWorker demonstrates S06 CLR closure fix.
// Registered as hosted service — runs for the lifetime of the application.
builder.Services.AddHostedService<DataProcessingWorker>();

// ── API ───────────────────────────────────────────────────────────────────

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// ── Demo seed (sample only, not a production pattern) ─────────────────────
// Gives the background worker real batches to process on startup so the S06
// path is observable in the console transcript. Production feeds the bus
// from upstream publishers, never from the composition root.
var demoBus = app.Services.GetRequiredService<InMemoryMessageBus>();
demoBus.Enqueue("demo-001", """{"orderId":"order-1","total":100}""", "payments");
demoBus.Enqueue("demo-002", """{"orderId":"order-2","total":250}""", "payments");
demoBus.Enqueue("demo-003", "plain-text-heartbeat", "ops");

// Map all endpoints
app.MapPreferencesEndpoints();

// Payment endpoint
app.MapPost("/payments", async (
    ProcessPaymentRequest request,
    PaymentProcessor processor,
    CancellationToken ct) =>
{
    var result = await processor.ProcessAsync(request, ct);

    return result switch
    {
        { IsDuplicate: true }                    => Results.Ok(new { status = "duplicate" }),
        { IsSuccess: true }                      => Results.Created($"/payments/{result.PaymentId}",
                                                       new { id = result.PaymentId }),
        { ErrorCode: "GATEWAY_TIMEOUT" }
            or { ErrorCode: "GATEWAY_UNREACHABLE" } => Results.StatusCode(503),
        _                                        => Results.BadRequest(new { error = result.ErrorCode }),
    };
})
.WithName("ProcessPayment")
.Produces(201)
.Produces(400)
.Produces(503);

app.Run();
