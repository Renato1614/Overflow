using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SearchService.Data;
using Typesense;
using Typesense.Setup;
using Wolverine;
using Wolverine.RabbitMQ;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var typesenseUri = builder.Configuration["services:typesense:http:0"];

if(string.IsNullOrEmpty(typesenseUri)) 
    throw new InvalidOperationException("Typesense URI not found in configuration");

var typesenseApiKey = builder.Configuration["typesense-api-key"];
if(string.IsNullOrEmpty(typesenseApiKey)) 
    throw new InvalidOperationException("Typesense api key not found in configuration");


var uri = new Uri(typesenseUri);
builder.Services.AddTypesenseClient(config =>
{
    config.ApiKey = typesenseApiKey;
    config.Nodes = new List<Node>
    {
        new(uri.Host, uri.Port.ToString(), uri.Scheme)
    };
});

builder.Services.AddOpenTelemetry().WithTracing(traceProviderBuilder =>
{
    traceProviderBuilder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                                           .AddService(builder.Environment.ApplicationName))
                                                            .AddSource("Wolverine");
});

builder.Host.UseWolverine(opts =>
{
    opts.UseRabbitMqUsingNamedConnection("messaging")
        .AutoProvision();

    opts.ListenToRabbitQueue("questions.search", cfg =>
    {
        cfg.BindExchange("questions");
    });

});

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapDefaultEndpoints();

using var scope = app.Services.CreateScope();
var client = scope.ServiceProvider.GetRequiredService<ITypesenseClient>();
await SearchInitializer.EnsureIndexExists(client);

app.Run();
