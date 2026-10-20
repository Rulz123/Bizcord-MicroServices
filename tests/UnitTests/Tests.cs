using ChannelManagement.Api.Messaging;
using ChannelManagementMicroservice.Models;
using FluentAssertions;
using Moq;
using ChannelManagementService.Context;
using Microsoft.EntityFrameworkCore;

namespace UnitTests;

public class Tests
{
    private readonly Mock<IMessageClient> _mockMessageClient;
    private readonly ChannelDbContext _dbContext;
    private readonly MessagePostedHandler _handler;
    private Channels channel;
    private MessagePostedEvent message;

    public Tests()
    {
        _mockMessageClient = new Mock<IMessageClient>();

        var options = new DbContextOptionsBuilder<ChannelDbContext>()
            .UseSqlServer("Data Source=127.0.0.1\\sqlserver,1433;Initial Catalog=ChannelManagementDB;Persist Security Info=True;User ID=sa;Password=Password123!;Pooling=False;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=True;Application Name=\"SQL Server Management Studio\";Command Timeout=0")
            .Options;

        _dbContext = new ChannelDbContext(options);
        _handler = new MessagePostedHandler(_mockMessageClient.Object, _dbContext);

        channel = new()
        {
            id = Guid.NewGuid(),
            newMessageCount = 0,
            categories = "TestCategory",
            description = "TestDescription",
            serverId = new Guid(),
            title = "TestTitle",
            type = "TestType",
            userroles = "TestUserRoles",
            visibility = true,
            writerights = "TestWriteRights"
        };

        message = new()
        {
            MessageId = Guid.NewGuid(),
            ChannelId = channel.id,
            AuthorId = Guid.NewGuid(),
            Content = "Hello world",
            PostedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task Handle_PublishesResultEvent_WithCorrectMessageId()
    {  
        _dbContext.Channels.Add(channel);
        await _dbContext.SaveChangesAsync();      
          
        channel = await _dbContext.Channels
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.id == message.ChannelId);
    
        await _handler.Handle(message, CancellationToken.None);

        _mockMessageClient.Verify(
            client => client.PublishAsync(
                It.Is<MessagePostedEvent>(published =>
                    published.MessageId == message.MessageId)),
            Times.Once);
    }
}