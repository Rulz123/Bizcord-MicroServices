# Bizcord-MicroServices
Skole projekt

See [Observability](Dokumentation/Observability.md) for OpenTelemetry, Prometheus, Docker
startup commands, configurable ports, and demonstration steps.

## Channel Management API

The API, CRUD controller/service, models, EF context, and migrations live in
`apps/channel-management-microservice/src/ChannelManagement.Api`.

Run from the repository root (SQL Server is the default):

```sh
dotnet run --project apps/channel-management-microservice/src/ChannelManagement.Api --launch-profile http
```

For local CRUD practice without SQL Server:

```sh
dotnet run --project apps/channel-management-microservice/src/ChannelManagement.Api --launch-profile http -- --Database:Provider=InMemory
```

The HTTP profile listens on `http://localhost:5041`. CRUD endpoints use
`/api/channels`. InMemory records disappear when the API stops. SQL Server
connection settings can be overridden with `ConnectionStrings__DefaultConnection`.
