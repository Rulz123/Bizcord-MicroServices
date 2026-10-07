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
BIZCORD_API_PORT=18081 BIZCORD_JAEGER_PORT=16688 docker compose up --build -d
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