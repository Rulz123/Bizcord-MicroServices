# Bizcord-MicroServices
Skole projekt

## Run the API in Docker (#7)

Requires Docker with Docker Compose. The local stack includes the .NET 10 API,
RabbitMQ, Jaeger, and Prometheus. Run from the repository root:

```sh
docker compose up --build -d
curl --fail --retry 10 --retry-all-errors --retry-delay 1 http://localhost:18080/health
```

| Service | Local URL | Host port override |
| --- | --- | --- |
| API health | http://localhost:18080/health | `BIZCORD_API_PORT` |
| RabbitMQ management | http://localhost:15672 | Not configured |
| Jaeger | http://localhost:16687 | `BIZCORD_JAEGER_PORT` |
| Prometheus | http://localhost:19090 | `BIZCORD_PROMETHEUS_PORT` |

The API runs as a non-root user in Production, without Swagger. `/health` returns
`Healthy` for application liveness; it does not check SQL or RabbitMQ connectivity.
Compose configures `ConnectionStrings__RabbitMq` to use `rabbitmq` on the internal
network and waits for RabbitMQ's healthcheck before starting the API. The local
RabbitMQ login is `guest` / `guest`; these defaults are not for production use.
RabbitMQ also publishes its AMQP port at `localhost:5672`.

SQL Server is not included. Set `ConnectionStrings__DefaultConnection` in the API
service's `environment` section to a reachable SQL Server before using
database-dependent requests. Inside Docker, `localhost` refers to the API
container, not the host or another container. Database provisioning and migrations
must be managed separately.

For a different API port, run `BIZCORD_API_PORT=18081 docker compose up --build -d`
and use http://localhost:18081/health. View logs with
`docker compose logs -f channel-management-api`. Stop the stack with
`docker compose down` (using the same port overrides if applicable).

## Verification

```sh
dotnet build apps/channel-management-microservice/src/ChannelManagement.Api/ChannelManagement.Api.csproj
docker compose config --quiet
```

There is currently no runnable test project. SQL requests need an external database.
OpenTelemetry registration is present, but the current API does not map `/metrics`,
so Prometheus scraping cannot succeed until that endpoint is restored.

See [Observability](Dokumentation/Observability.md) for OpenTelemetry, Prometheus, Docker
startup commands, configurable ports, and demonstration steps.
