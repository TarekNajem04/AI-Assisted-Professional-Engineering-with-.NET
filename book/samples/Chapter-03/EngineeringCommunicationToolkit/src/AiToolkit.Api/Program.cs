// book/samples/Chapter-03/EngineeringCommunicationToolkit/src/AiToolkit.Api/Program.cs
//
// Composition root — wires Core, Application, Infrastructure, and Api.
// Follows the same layering as Chapter 02's AiAwareEngineeringToolkit.

using AiToolkit.Api.Endpoints;
using AiToolkit.Application;
using AiToolkit.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.MapWorkflowEndpoints();

app.Run();
