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

## Docker deployment

Docker is used for the W39 local deployment increment. You need Docker Engine, Docker Compose and the .NET SDK installed locally before running the commands below.

Create a local environment file from the committed template:

```bash
cp .env.example .env
```

The `.env` file is ignored by Git. It supplies local RabbitMQ credentials to Compose and must not contain production secrets.

Validate the Compose file and resolved environment references:

```bash
docker compose config
```

Build the standalone API image:

```bash
docker build -t bizcord-channel-management:local .
```

Build and start the full local environment:

```bash
docker compose build
docker compose up -d
```

Inspect status and logs:

```bash
docker compose ps
docker compose logs --no-color rabbitmq
docker compose logs --no-color channel-management
```

The API is exposed on:

```text
http://127.0.0.1:5088/health
```

RabbitMQ Management is exposed on:

```text
http://127.0.0.1:15672
```

Stop the local environment:

```bash
docker compose down
```

Use this after source changes to rebuild and restart:

```bash
docker compose build
docker compose up -d
```

### Docker build and NuGet connectivity

In one verified Ubuntu development environment, DNS resolution from Docker's default bridge network worked, but HTTPS requests to `https://api.nuget.org/v3/index.json` timed out. This caused `dotnet restore` inside the Docker build to fail with `NU1301`, even though restore worked directly on the host.

The Dockerfile was verified with this one-time diagnostic command:

```bash
docker build --network=host --progress=plain \
	-t bizcord-channel-management:local .
```

`--network=host` is an environment-specific diagnostic workaround, not the project's default or a portable requirement. Developers whose Docker bridge network can reach NuGet should use the normal `docker build` or `docker compose build` commands.

If this problem occurs, compare host and container access to NuGet and repair the local Docker networking configuration outside this repository. Do not add runtime host networking to the application services.

### Container service discovery

Inside Docker Compose, the API connects to RabbitMQ with the hostname `rabbitmq`. That name is resolved by Compose DNS to the RabbitMQ service container. The API must not use `localhost` in the container because `localhost` would point back to the API container itself, not the RabbitMQ container.

The `channel-management` service uses `depends_on` with `condition: service_healthy`, and RabbitMQ has a `rabbitmq-diagnostics -q ping` health check. This means Compose waits until RabbitMQ reports healthy before starting the API container.

Startup ordering is not a complete runtime resilience strategy. RabbitMQ can still become unavailable after startup. Retries, idempotency, dead-letter queues and outbox support remain future work.

## Current limitations

- Channel persistence is in-memory only.
- The liveness health endpoint does not claim RabbitMQ is available.
- Channel creation publishes after persistence but does not yet use an outbox, so a publish failure after persistence can leave data and messages inconsistent.
- There is no API gateway, authentication, authorization or database persistence in this increment.
- Docker Compose startup ordering does not protect against broker outages after startup.
- Full producer-to-consumer verification is not included in this Docker increment.

## Next increment

Add a future microservice testing increment for producer-to-broker behavior and future consumer integration scenarios.
