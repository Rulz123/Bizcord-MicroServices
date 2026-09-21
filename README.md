# Bizcord-MicroServices
Skole projekt

## Channel Management service

The Channel Management microservice owns channel operations for Bizcord guilds. In the current scaffold it supports text, voice and forum channels, keeps channel state in an in-memory repository, exposes REST endpoints, and publishes a `ChannelCreatedV1` integration event after a channel is created.

## Solution structure

- `src/Bizcord.ChannelManagement.Domain`: channel entity, channel type, status and settings. This layer has no project dependencies and contains no infrastructure code.
- `src/Bizcord.ChannelManagement.Contracts`: immutable, transport-neutral models shared with other services.
- `src/Bizcord.ChannelManagement.Application`: use-case services, repository abstraction, explicit mapping to external contracts, and the `IMessageClient` abstraction.
- `src/Bizcord.ChannelManagement.Infrastructure`: in-memory repository and EasyNetQ-backed messaging adapter.
- `src/Bizcord.ChannelManagement.Api`: Minimal API composition root, endpoint registration, health endpoint and configuration.
- `tests/Bizcord.ChannelManagement.Tests`: focused application and infrastructure configuration tests.

## Dependency rules

The intended dependency direction is:

```text
Domain
	no project dependencies

Contracts
	no project dependencies

Application
	-> Domain
	-> Contracts

Infrastructure
	-> Application
	-> Contracts

Api
	-> Application
	-> Infrastructure
	-> Contracts
```

Domain remains technology-agnostic. EasyNetQ and RabbitMQ details are isolated in Infrastructure. The API is the composition root and should not contain business logic.

## Internal model and external contracts

The internal domain model can contain behavior and implementation details that are useful inside the service. External contracts are stable integration messages intended for other microservices. The service maps explicitly from internal channel data to contracts such as `ChannelSummaryV1` and `ChannelCreatedV1`; it does not expose domain entities directly.

## Messaging

Application code depends on `IMessageClient`, a small transport-neutral abstraction for asynchronous publish and subscribe operations with cancellation support. Infrastructure implements that abstraction with EasyNetQ.

`ChannelService.CreateChannelAsync` persists the channel first, then publishes exactly one `ChannelCreatedV1` event. Publishing is awaited and failures are not swallowed. Atomic database-and-message consistency is not solved yet because this increment intentionally does not add an outbox.

RabbitMQ is configured with a safe local-development default:

```json
{
	"RabbitMq": {
		"ConnectionString": "host=localhost"
	}
}
```

Do not commit real production credentials. Override the connection string through environment-specific configuration or secret management.

## REST and messaging learning notes

Synchronous REST is request/response communication: the caller waits for the service to finish and return a result. Asynchronous messaging lets the service publish a fact, such as `ChannelCreatedV1`, and other services react independently.

RabbitMQ is the message broker that routes and stores messages between services. EasyNetQ is the .NET client library used here to publish and subscribe without putting RabbitMQ-specific code into Application.

Interfaces and stable contracts reduce coupling. Application depends on `IMessageClient`, while other services depend on versioned contracts instead of internal domain classes.

Reliable message handling is future work. Production messaging should include retries, idempotent consumers, dead-letter queues and an outbox or similar pattern for database-and-message consistency.

## Commands

Restore dependencies:

```bash
dotnet restore Bizcord.MicroServices.sln
```

Build:

```bash
dotnet build Bizcord.MicroServices.sln --no-restore
```

Run tests:

```bash
dotnet test Bizcord.MicroServices.sln --no-build
```

Run the API locally:

```bash
dotnet run --project src/Bizcord.ChannelManagement.Api/Bizcord.ChannelManagement.Api.csproj --urls http://127.0.0.1:5088
```

Health check:

```bash
curl -i http://127.0.0.1:5088/health
```

## Current limitations

- Channel persistence is in-memory only.
- The liveness health endpoint does not claim RabbitMQ is available.
- Channel creation publishes after persistence but does not yet use an outbox, so a publish failure after persistence can leave data and messages inconsistent.
- There is no Dockerfile, Docker Compose setup, API gateway, authentication, authorization or database persistence in this increment.

## Next increment

Add a Dockerfile and Docker Compose setup with RabbitMQ, including RabbitMQ health checks and service startup ordering suitable for local integration testing.
