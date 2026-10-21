# Bizcord-MicroServices
Skole projekt

## Run the API in Docker (#7)

Requires Docker with Docker Compose. The local stack includes the .NET 10 API,
SQL Server, RabbitMQ, Jaeger, and Prometheus. Run from the repository root:

**First-time setup:** follow [Database setup and migrations](#database-setup-and-migrations)
below before using CRUD endpoints or running tests. Container health does not
mean the application database and tables have been created.

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
network and waits for SQL Server and RabbitMQ healthchecks before starting the API. The local
RabbitMQ login is `guest` / `guest`; these defaults are not for production use.
RabbitMQ also publishes its AMQP port at `localhost:5672`.

SQL Server is included in Compose, with data persisted in the `sqlserver-data`
volume. `ConnectionStrings__DefaultConnection` points the API to `sqlserver:1433`.
Inside Docker, `localhost` refers to the API container, not the host or another
container. Migrations must still be applied separately; see the setup steps below.

For a different API port, run `BIZCORD_API_PORT=18081 docker compose up --build -d`
and use http://localhost:18081/health. View logs with
`docker compose logs -f channel-management-api`. Stop the stack with
`docker compose down` (using the same port overrides if applicable).

## Verification

```sh
dotnet build apps/channel-management-microservice/src/ChannelManagement.Api/ChannelManagement.Api.csproj
docker compose config --quiet
```

The HTTP integration test project contains 14 CRUD test cases. It requires the
running Compose API and migrated SQL database; see [Run CRUD tests](#run-crud-tests).
The API exposes `/metrics` for Prometheus scraping.

See [Observability](docs/Observability.md) for OpenTelemetry, Prometheus, Docker
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


## CRUD feature

Channel Management manages channel records across Bizcord servers. This feature
adds POST, GET (single and list), PUT, and DELETE at `/api/channels`, using a
controller, service, request models, EF Core context, and SQL migrations. It
validates titles, channel types, and the server ID, generates channel IDs, and
preserves channel/server identity during updates.

See [Channel CRUD feature and test coverage](docs/channel-crud.md) for endpoint
responses, examples, current limitations, and the 14 test cases.

## Database setup and migrations

**Apply migrations before database-dependent requests or CRUD tests.** Starting a
SQL container creates a running database engine, not the `ChannelManagementDB`
schema. `/health` checks application liveness only. The API and tests do not
apply migrations automatically.

Prerequisites: Docker Desktop with Linux containers, .NET 10 SDK, and EF CLI
10.0.12. Run all commands from the repository root. Check the EF tool:

```powershell
dotnet ef --version
```

If it is not installed:

```powershell
dotnet tool install --global dotnet-ef --version 10.0.12
```

1. Start infrastructure:

   ```powershell
   docker compose up -d sqlserver rabbitmq
   docker compose ps
   ```

   Wait until both report `healthy`. Stop any old standalone SQL container using
   port 1433 first. Its database is separate from the Compose data volume.

2. Apply the existing migration:

   ```powershell
   dotnet ef database update --project apps/channel-management-microservice/src/ChannelManagement.Api --connection "Server=tcp:127.0.0.1,1433;Database=ChannelManagementDB;User Id=sa;Password=Password123!;TrustServerCertificate=True" -- --environment Development
   ```

   Expect `Done.` before continuing. This creates the database if needed and
   applies pending migrations, including the `Channels` table. The explicit IPv4
   TCP address avoids the localhost connection timeout encountered during setup.
   The credentials match this local school's Compose configuration.

3. Build and start the complete stack:

   ```powershell
   docker compose up --build -d
   ```

Repeat the migration command after pulling new migrations or using a fresh SQL
volume. Applied migrations are recorded by EF; they are not reapplied on every
restart. Do not create another migration merely to initialize a new database.

`docker compose down` retains SQL data. `docker compose down -v` removes the SQL
volume and its records, requiring migrations again. Use API-only restart for a
persistence check: `docker compose restart channel-management-api`.

For SQL timeouts, check container health, published port 1433, and whether another
SQL container owns that port. An unavailable `ChannelManagementDB` or missing
`Channels` table on a fresh volume indicates migrations still need applying.

## Run CRUD tests

With SQL Server and RabbitMQ healthy, the migration applied, and the Compose API
running:

```powershell
dotnet test apps/channel-management-microservice/tests/ChannelManagement.Api.IntegrationTests
```

Expected result: **14 passed**. These tests send real HTTP requests to
`http://localhost:18080` and use the API's SQL database. They do not launch the
API, start containers, or apply migrations. They create temporary records and
attempt cleanup; an interrupted test run can leave records behind.

The test base URL is currently hardcoded. Use Compose's default API port 18080,
or update the test base addresses if changing `BIZCORD_API_PORT`. Running
`dotnet test` does not rebuild the running Docker API image: after API code
changes, run `docker compose up --build -d` first.

The suite covers Create/Read, Update, Delete, listing, missing records, and
validation, including preservation of stored data after invalid updates. It does
not cover messaging contracts or automatically test persistence across restarts.
The API restart persistence check was performed manually during development.

When running the API directly through VS Code instead of Compose, start SQL and
RabbitMQ and apply migrations first. InMemory practice skips SQL migrations but
still requires RabbitMQ and cannot verify SQL persistence.


## Message-count migration and handler test

The merged message handler adds `Channels.newMessageCount`. Apply pending
migrations again after pulling this change, including
`20261020120016_DatabaseV_0.1`, before rebuilding and using the API. Existing
records receive a count of zero. Use the migration command in the setup section;
do not remove the SQL volume to update the schema.

The message-handler test is separate from the 14 HTTP CRUD cases and uses an
isolated EF InMemory database with a mocked message client:

```powershell
dotnet test tests/UnitTests
```

It verifies that handling a message persists an incremented channel message count
and publishes the expected message fields. It does not verify real broker delivery.
The HTTP CRUD tests still require the running, rebuilt API and migrated SQL database.
Expected result: **6** passed in total with Contract tests.
