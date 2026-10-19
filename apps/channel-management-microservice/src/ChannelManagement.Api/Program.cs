//Libraries
using ChannelManagementService.Context;
using Microsoft.EntityFrameworkCore;

//References
using ChannelManagement.Api.Messaging.Requests;
using ChannelManagement.Api.Messaging;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
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

var app = builder.Build();

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
