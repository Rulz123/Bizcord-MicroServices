# Observability

## Start

Run from the repository root:

```sh
docker compose up --build -d
curl -f http://localhost:18080/health
```

The API runs at http://localhost:18080 and Jaeger at http://localhost:16687.
Host ports are configurable without changing internal container addresses:

```sh
BIZCORD_API_PORT=18081 BIZCORD_JAEGER_PORT=16688 BIZCORD_PROMETHEUS_PORT=19091 docker compose up --build -d
```

Use the corresponding host ports in subsequent curl commands. Stop with
`docker compose down`, using the same overrides if applicable.

## OpenTelemetry (#6)

The service `Bizcord.ChannelManagementService` instruments ASP.NET Core
requests and outgoing HttpClient calls. HTTP request and .NET runtime metrics
are exposed at `/metrics`. `OpenTelemetry__ServiceName` can override the name.
Traces are exported over OTLP/gRPC to `http://jaeger:4317` on the Compose network.
Jaeger stores traces in memory; recreating the container clears them.

```sh
for request in 1 2 3; do curl -fsS http://localhost:18080/health; done
curl -fsS http://localhost:18080/metrics
curl -fsSG http://localhost:16687/api/traces \
  --data-urlencode 'service=Bizcord.ChannelManagementService' \
  --data-urlencode 'operation=GET /health' \
  --data-urlencode 'lookback=1h'
```

Allow a few seconds for the trace batch exporter. In Jaeger select the service
and search for `GET /health`.

SQL/EF registration and Swagger are preserved. `/health` only checks application
liveness, not SQL connectivity. No SQL instance is started by this observability
stack. Provide a real connection string through deployment configuration before
using database-dependent endpoints; the existing local development configuration
is unchanged. The API container runs in Production, so Swagger is not exposed.
HttpClient instrumentation is configured, but this scaffold has no outgoing calls
to demonstrate. The Prometheus ASP.NET Core exporter is pinned to a beta release.

## Prometheus (#13)

Open http://localhost:19090/targets and confirm `channel-management-api` is `UP`.
`BIZCORD_PROMETHEUS_PORT` overrides the host port. Prometheus scrapes
`http://channel-management-api:8080/metrics` every five seconds; host port
overrides do not change this address. No credentials are required by this local
observability configuration. Ports are bound to loopback only.

```sh
curl -fsS http://localhost:19090/api/v1/targets
curl -fsSG http://localhost:19090/api/v1/query \
  --data-urlencode 'query=up{job="channel-management-api"}'
curl -fsSG http://localhost:19090/api/v1/query \
  --data-urlencode 'query=http_server_request_duration_seconds_count{job="channel-management-api",http_route="/health"}'
```

The `up` query should return `1`. Record the request count, send three more
`curl -fsS http://localhost:18080/health` requests, then repeat the count query
after the next scrape. It should increase by three. The counter resets when
the API restarts. Metrics for `/metrics` itself are also recorded, so use the
`/health` route filter. This local stack does not configure durable storage for
Prometheus or Jaeger.

## Checks

```sh
dotnet build Application/ChannelManagementService/ChannelManagementService.csproj
docker compose config --quiet
docker compose exec -T prometheus promtool check config /etc/prometheus/prometheus.yml
```

The Test baseline has no runnable test project. SQL connectivity and outgoing
HttpClient traces are not covered by this demonstration. Existing nullable-model
warnings are outside these observability issues.