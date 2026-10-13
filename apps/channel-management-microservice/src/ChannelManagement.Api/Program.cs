var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


//CHECK_______________________________________________________________________________
var s = new ChannelManagement.Api.Messaging.JsonMessageSerializer();
var original = new ChannelManagement.Api.Messaging.Events.ChannelCreated(
    Guid.NewGuid(), Guid.NewGuid(), "general", "Main chat", "text", "text", "everyone", "everyone", true);
var bytes = s.Serialize(original);
Console.WriteLine(System.Text.Encoding.UTF8.GetString(bytes));
Console.WriteLine(s.Deserialize<ChannelManagement.Api.Messaging.Events.ChannelCreated>(bytes) == original);
//___________________________________________________________________________________

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
