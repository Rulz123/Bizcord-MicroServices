using Bizcord.ChannelManagement.Domain;

namespace Bizcord.ChannelManagement.Application;

public interface IChannelRepository
{
    Task<IReadOnlyCollection<Channel>> ListAsync(Guid? guildId, CancellationToken cancellationToken);

    Task<Channel?> GetAsync(Guid id, CancellationToken cancellationToken);

    Task SaveAsync(Channel channel, CancellationToken cancellationToken);
}