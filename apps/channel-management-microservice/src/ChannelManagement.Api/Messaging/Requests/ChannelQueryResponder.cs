//Libraries
using Microsoft.EntityFrameworkCore;

//References
using ChannelManagementService.Context;

namespace ChannelManagement.Api.Messaging.Requests;

public class ChannelQueryResponder(IMessageClient client, IServiceScopeFactory scopes) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await client.RespondAsync<ChannelsAndCategoriesRequest, ChannelsAndCategoriesReply>(async request =>
        {
            using var scope = scopes.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ChannelDbContext>();

            var channels = await db.Channels
                .Where(c => c.serverId == request.ServerId)
                .ToListAsync();

            var categories = channels
                .GroupBy(c => c.categories)
                .Select(g => new CategoryDto(g.Key,
                    g.Select(c => new ChannelDto(c.id, c.title, c.description, c.type, c.visibility)).ToList()))
                .ToList();

            return new ChannelsAndCategoriesReply(request.ServerId, categories);
        }, ct);
    }
}