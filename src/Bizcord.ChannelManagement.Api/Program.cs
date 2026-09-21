using Bizcord.ChannelManagement.Application;
using Bizcord.ChannelManagement.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ChannelService>();
builder.Services.AddChannelManagementInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health")
    .WithName("HealthCheck");

var channelGroup = app.MapGroup("/api/channels")
    .WithTags("Channels")
    .WithOpenApi();

channelGroup.MapGet("/", async (Guid? guildId, ChannelService service, CancellationToken cancellationToken) =>
{
    var channels = await service.GetChannelsAsync(guildId, cancellationToken);
    return Results.Ok(channels);
})
.WithName("ListChannels");

channelGroup.MapGet("/{id:guid}", async (Guid id, ChannelService service, CancellationToken cancellationToken) =>
{
    var channel = await service.GetChannelAsync(id, cancellationToken);
    return channel is null ? Results.NotFound() : Results.Ok(channel);
})
.WithName("GetChannel");

channelGroup.MapPost("/", async (CreateChannelRequest request, ChannelService service, CancellationToken cancellationToken) =>
{
    try
    {
        var channel = await service.CreateChannelAsync(request, cancellationToken);
        return Results.Created($"/api/channels/{channel.Id}", channel);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
})
.WithName("CreateChannel");

channelGroup.MapPut("/{id:guid}", async (Guid id, UpdateChannelRequest request, ChannelService service, CancellationToken cancellationToken) =>
{
    try
    {
        var channel = await service.UpdateChannelAsync(id, request, cancellationToken);
        return channel is null ? Results.NotFound() : Results.Ok(channel);
    }
    catch (ArgumentException exception)
    {
        return Results.BadRequest(new { error = exception.Message });
    }
    catch (InvalidOperationException exception)
    {
        return Results.Conflict(new { error = exception.Message });
    }
})
.WithName("UpdateChannel");

channelGroup.MapDelete("/{id:guid}", async (Guid id, ChannelService service, CancellationToken cancellationToken) =>
{
    var archived = await service.ArchiveChannelAsync(id, cancellationToken);
    return archived ? Results.NoContent() : Results.NotFound();
})
.WithName("ArchiveChannel");

app.Run();
