// book/samples/Chapter-02/UnderstandingLLMs/src/AiToolkit.Api/Program.cs
//
// Composition root — wires Core, Application, Infrastructure, and Api
// together. Same pattern as Chapter 01's PaymentService.Api/Program.cs
// for architectural consistency across the book.

using AiToolkit.Api.Endpoints;
using AiToolkit.Application;
using AiToolkit.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapAiSuggestionEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

 await app.RunAsync();
