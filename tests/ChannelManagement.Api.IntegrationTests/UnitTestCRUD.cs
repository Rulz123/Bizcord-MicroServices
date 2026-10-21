using System.Net;
using System.Net.Http.Json;
using ChannelManagement.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace ChannelManagement.Api.IntegrationTests;

public class ChannelCrudTests
{
    [Fact]
    public async Task CreateChannel_CanBeRetrievedById()
    {
        // Arrange
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var request = new CreateChannelRequest
        {
            ServerId = Guid.NewGuid(),
            Title = "automated-test",
            Type = "Text",
            Categories = "Testing",
            Visibility = true,
            Description = "Created by an integration test"
        };

        Guid? createdId = null;

        try
        {
            // Act: create the channel
            using var createResponse =
                await client.PostAsJsonAsync("/api/channels", request);

            // Assert: creation succeeded
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created =
                await createResponse.Content.ReadFromJsonAsync<Channels>();

            Assert.NotNull(created);
            Assert.NotEqual(Guid.Empty, created.id);

            createdId = created.id;

            // Act: retrieve the saved channel
            using var getResponse =
                await client.GetAsync($"/api/channels/{createdId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var retrieved =
                await getResponse.Content.ReadFromJsonAsync<Channels>();

            Assert.NotNull(retrieved);
            Assert.Equal(created.id, retrieved.id);
            Assert.Equal(request.ServerId, retrieved.serverId);
            Assert.Equal(request.Title, retrieved.title);
            Assert.Equal(request.Type, retrieved.type);
            Assert.Equal(request.Description, retrieved.description);
        }
        finally
        {
            // Remove only the record created by this test
            if (createdId.HasValue)
            {
                using var cleanupResponse =
                    await client.DeleteAsync($"/api/channels/{createdId}");
            }
        }
    }

    [Fact]
    public async Task UpdateChannel_ChangesSettingsAndPreservesIdentity()
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var createRequest = new CreateChannelRequest
        {
            ServerId = Guid.NewGuid(),
            Title = "before-update",
            Type = "Text",
            Categories = "Testing",
            Visibility = true
        };

        Guid? createdId = null;

        try
        {
            // Arrange: create a channel to update
            using var createResponse =
                await client.PostAsJsonAsync("/api/channels", createRequest);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created =
                await createResponse.Content.ReadFromJsonAsync<Channels>();

            Assert.NotNull(created);
            createdId = created.id;

            var updateRequest = new UpdateChannelRequest
            {
                Title = "after-update",
                Type = "Voice",
                Categories = "Updated category",
                UserRoles = "Member",
                Visibility = false,
                WriteRights = "Moderator",
                Description = "Updated description"
            };

            // Act
            using var updateResponse = await client.PutAsJsonAsync(
                $"/api/channels/{createdId}",
                updateRequest);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            using var getResponse =
                await client.GetAsync($"/api/channels/{createdId}");

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var updated =
                await getResponse.Content.ReadFromJsonAsync<Channels>();

            Assert.NotNull(updated);

            Assert.Equal(created.id, updated.id);
            Assert.Equal(createRequest.ServerId, updated.serverId);

            Assert.Equal(updateRequest.Title, updated.title);
            Assert.Equal(updateRequest.Type, updated.type);
            Assert.Equal(updateRequest.Categories, updated.categories);
            Assert.Equal(updateRequest.UserRoles, updated.userroles);
            Assert.Equal(updateRequest.Visibility, updated.visibility);
            Assert.Equal(updateRequest.WriteRights, updated.writerights);
            Assert.Equal(updateRequest.Description, updated.description);
        }
        finally
        {
            if (createdId.HasValue)
            {
                using var cleanupResponse =
                    await client.DeleteAsync($"/api/channels/{createdId}");
            }
        }
    }

    [Fact]
    public async Task DeleteChannel_RemovesChannelAndReturns404Afterwards()
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var request = new CreateChannelRequest
        {
            ServerId = Guid.NewGuid(),
            Title = "channel-to-delete",
            Type = "Text",
            Categories = "Testing",
            Visibility = true
        };

        Guid? createdId = null;

        try
        {
            // Arrange
            using var createResponse =
                await client.PostAsJsonAsync("/api/channels", request);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created =
                await createResponse.Content.ReadFromJsonAsync<Channels>();

            Assert.NotNull(created);
            createdId = created.id;

            var channelUrl = $"/api/channels/{createdId}";

            // Act
            using var deleteResponse = await client.DeleteAsync(channelUrl);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            using var getResponse = await client.GetAsync(channelUrl);
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);

            using var secondDeleteResponse = await client.DeleteAsync(channelUrl);
            Assert.Equal(HttpStatusCode.NotFound, secondDeleteResponse.StatusCode);
        }
        finally
        {
            if (createdId.HasValue)
            {
                using var cleanupResponse =
                    await client.DeleteAsync($"/api/channels/{createdId}");
            }
        }
    }

    [Theory]
    [InlineData("", "Text", "Title")]
    [InlineData("   ", "Text", "Title")]
    [InlineData("valid-title", "", "Type")]
    [InlineData("valid-title", "Video", "Type")]
    public async Task CreateChannel_InvalidInput_ReturnsValidationError(
        string title,
        string type,
        string expectedErrorField)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var request = new CreateChannelRequest
        {
            ServerId = Guid.NewGuid(),
            Title = title,
            Type = type
        };

        using var response =
            await client.PostAsJsonAsync("/api/channels", request);

        // Clean up if a regression accidentally allows creation
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var created =
                await response.Content.ReadFromJsonAsync<Channels>();

            if (created is not null)
            {
                using var cleanupResponse =
                    await client.DeleteAsync($"/api/channels/{created.id}");
            }
        }

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey(expectedErrorField));
    }

    [Fact]
    public async Task CreateChannel_EmptyServerId_ReturnsValidationError()
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var request = new CreateChannelRequest
        {
            ServerId = Guid.Empty,
            Title = "invalid-server-test",
            Type = "Text"
        };

        using var response =
            await client.PostAsJsonAsync("/api/channels", request);

        // Clean up if a regression accidentally allows creation
        if (response.StatusCode == HttpStatusCode.Created)
        {
            var created =
                await response.Content.ReadFromJsonAsync<Channels>();

            if (created is not null)
            {
                using var cleanupResponse =
                    await client.DeleteAsync($"/api/channels/{created.id}");
            }
        }

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem =
            await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();

        Assert.NotNull(problem);
        Assert.True(problem.Errors.ContainsKey("ServerId"));
    }

    [Fact]
    public async Task UpdateChannel_MissingId_Returns404WithoutCreatingChannel()
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var missingId = Guid.NewGuid();
        var channelUrl = $"/api/channels/{missingId}";

        var request = new UpdateChannelRequest
        {
            Title = "missing-channel-test",
            Type = "Text",
            Categories = "Testing",
            Visibility = true
        };

        // Confirm the test ID doesn't exist
        using var beforeResponse = await client.GetAsync(channelUrl);
        Assert.Equal(HttpStatusCode.NotFound, beforeResponse.StatusCode);

        // Attempt the update
        using var updateResponse =
            await client.PutAsJsonAsync(channelUrl, request);

        // Verify the result and absence of a new record
        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        using var afterResponse = await client.GetAsync(channelUrl);
        Assert.Equal(HttpStatusCode.NotFound, afterResponse.StatusCode);
    }

    [Theory]
    [InlineData("", "Text", "Title")]
    [InlineData("   ", "Text", "Title")]
    [InlineData("changed-title", "", "Type")]
    [InlineData("changed-title", "Video", "Type")]
    public async Task UpdateChannel_InvalidInput_LeavesChannelUnchanged(
        string title,
        string type,
        string expectedErrorField)
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var createRequest = new CreateChannelRequest
        {
            ServerId = Guid.NewGuid(),
            Title = "original-title",
            Type = "Text",
            Categories = "Original category",
            Visibility = true,
            Description = "Original description"
        };

        Guid? createdId = null;

        try
        {
            // Arrange: create a valid channel
            using var createResponse =
                await client.PostAsJsonAsync("/api/channels", createRequest);

            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            var created =
                await createResponse.Content.ReadFromJsonAsync<Channels>();

            Assert.NotNull(created);
            createdId = created.id;

            var channelUrl = $"/api/channels/{createdId}";

            var updateRequest = new UpdateChannelRequest
            {
                Title = title,
                Type = type,
                Categories = "Changed category",
                Visibility = false,
                Description = "Changed description"
            };

            // Act
            using var updateResponse =
                await client.PutAsJsonAsync(channelUrl, updateRequest);

            // Assert: the invalid request was rejected
            Assert.Equal(HttpStatusCode.BadRequest, updateResponse.StatusCode);

            var problem = await updateResponse.Content
                .ReadFromJsonAsync<ValidationProblemDetails>();

            Assert.NotNull(problem);
            Assert.True(problem.Errors.ContainsKey(expectedErrorField));

            // Verify that no settings were changed
            using var getResponse = await client.GetAsync(channelUrl);

            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var unchanged =
                await getResponse.Content.ReadFromJsonAsync<Channels>();

            Assert.NotNull(unchanged);
            Assert.Equal(created.id, unchanged.id);
            Assert.Equal(createRequest.ServerId, unchanged.serverId);
            Assert.Equal(createRequest.Title, unchanged.title);
            Assert.Equal(createRequest.Type, unchanged.type);
            Assert.Equal(createRequest.Categories, unchanged.categories);
            Assert.Equal(createRequest.Visibility, unchanged.visibility);
            Assert.Equal(createRequest.Description, unchanged.description);
        }
        finally
        {
            if (createdId.HasValue)
            {
                using var cleanupResponse =
                    await client.DeleteAsync($"/api/channels/{createdId}");
            }
        }
    }

    [Fact]
    public async Task GetChannels_IncludesCreatedChannelsAndExcludesDeletedChannel()
    {
        using var client = new HttpClient
        {
            BaseAddress = new Uri("http://localhost:18080")
        };

        var createdIds = new List<Guid>();
        var serverId = Guid.NewGuid();

        try
        {
            // Arrange: create two channels
            foreach (var title in new[] { "list-test-one", "list-test-two" })
            {
                var request = new CreateChannelRequest
                {
                    ServerId = serverId,
                    Title = title,
                    Type = "Text",
                    Categories = "Testing",
                    Visibility = true
                };

                using var createResponse =
                    await client.PostAsJsonAsync("/api/channels", request);

                Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

                var created =
                    await createResponse.Content.ReadFromJsonAsync<Channels>();

                Assert.NotNull(created);
                createdIds.Add(created.id);
            }

            // Act: retrieve the list
            using var listResponse = await client.GetAsync("/api/channels");

            Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

            var channels =
                await listResponse.Content.ReadFromJsonAsync<List<Channels>>();

            Assert.NotNull(channels);
            Assert.Contains(channels, channel => channel.id == createdIds[0]);
            Assert.Contains(channels, channel => channel.id == createdIds[1]);

            // Delete the first channel
            using var deleteResponse =
                await client.DeleteAsync($"/api/channels/{createdIds[0]}");

            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // Retrieve the list again
            using var updatedListResponse =
                await client.GetAsync("/api/channels");

            Assert.Equal(HttpStatusCode.OK, updatedListResponse.StatusCode);

            var remainingChannels = await updatedListResponse.Content
                .ReadFromJsonAsync<List<Channels>>();

            Assert.NotNull(remainingChannels);
            Assert.DoesNotContain(
                remainingChannels,
                channel => channel.id == createdIds[0]);

            Assert.Contains(
                remainingChannels,
                channel => channel.id == createdIds[1]);
        }
        finally
        {
            foreach (var id in createdIds)
            {
                using var cleanupResponse =
                    await client.DeleteAsync($"/api/channels/{id}");
            }
        }
    }

}