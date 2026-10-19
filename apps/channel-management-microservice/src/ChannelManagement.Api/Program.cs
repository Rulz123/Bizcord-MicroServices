//Libraries
using ChannelManagement.Api.Context;
using ChannelManagement.Api.Services;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

//References
using ChannelManagement.Api.Messaging.Requests;
using ChannelManagement.Api.Messaging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ChannelDbContext>(options =>
{
    if (builder.Configuration["Database:Provider"] == "InMemory")
    {
        options.UseInMemoryDatabase("BizcordChannels");
    }
    else
    {
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    }
});
builder.Services.AddScoped<ChannelService>();
// Add services to the container.

builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
builder.Services.AddHealthChecks();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();
builder.Services.AddDbContext<ChannelDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});
builder.Services.AddHostedService<ChannelQueryResponder>();

//RabbitMQ_______________________________________________________________________________
var rabbitMq = builder.Configuration.GetConnectionString("RabbitMq")
    ?? throw new InvalidOperationException("Missing ConnectionStrings:RabbitMq");

builder.Services.AddMessageClient(rabbitMq);
//_______________________________________________________________________________________


var serviceName = builder.Configuration["OpenTelemetry:ServiceName"]
    ?? "Bizcord.ChannelManagementService";

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddPrometheusExporter());

var app = builder.Build();

app.MapPrometheusScrapingEndpoint("/metrics");
app.MapHealthChecks("/health");

// TEMPORARY TEST - delete after it works
app.MapGet("/test-channels/{serverId:guid}", async (Guid serverId, IMessageClient client) =>
    await client.RequestAsync<ChannelsAndCategoriesRequest, ChannelsAndCategoriesReply>(
        new ChannelsAndCategoriesRequest(serverId)));


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
