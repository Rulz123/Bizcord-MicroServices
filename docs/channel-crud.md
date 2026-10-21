# Channel CRUD feature

## Scope and implementation

Channel Management manages channel records for all Bizcord servers. `serverId`
associates each record with its server. This feature provides Create, Read,
Update, and Delete over HTTP using EF Core and MSSQL.

Code locations:

- API: `apps/channel-management-microservice/src/ChannelManagement.Api`
- HTTP endpoints: `Controllers/ChannelManagementController.cs`
- CRUD operations and EF queries: `Services/ChannelService.cs`
- Stored entity and request models: `Models/`
- Database context: `Context/ChannelDbContext.cs`
- Schema migrations: `Migrations/`
- Tests: `tests/ChannelManagement.Api.IntegrationTests/UnitTestCRUD.cs`

The controller handles HTTP requests/responses and request validation. The
service maps inputs, queries records, and saves changes. Dependency injection
supplies the service and database context. The API generates channel IDs.

## HTTP contract

Default Compose base URL: `http://localhost:18080`.

| Method | Route | Success | Other expected results |
| --- | --- | --- | --- |
| POST | `/api/channels` | 201, created channel and Location header | 400 for invalid input |
| GET | `/api/channels` | 200, array (possibly empty) | |
| GET | `/api/channels/{id}` | 200, channel | 404 when absent |
| PUT | `/api/channels/{id}` | 204, no body | 400 for invalid input; 404 when absent |
| DELETE | `/api/channels/{id}` | 204, no body | 404 when absent, including repeated deletion |

Routes containing `{id}` require a GUID. List currently returns channels across
all servers; it has no server filter. Delete removes the channel record rather
than archiving it. It does not delete messages owned by another microservice.

Example Create JSON:

```json
{
  "serverId": "22222222-2222-2222-2222-222222222222",
  "title": "general",
  "type": "Text",
  "categories": "Community",
  "userRoles": "",
  "visibility": true,
  "writeRights": "",
  "description": "General discussion"
}
```

Create requires a nonempty server GUID, a nonblank title, and one of the exact
case-sensitive types `Voice`, `Text`, `Thread`, or `Forums`. The server GUID is not
currently checked against a server registry. Titles are trimmed before saving.
Other string fields may be empty; visibility defaults to false when omitted.

PUT uses the same editable settings, excluding `serverId`. The ID comes from the
URL. It replaces all editable settings: omitted optional fields take their
request-model defaults. It preserves both the channel ID and server ID. Invalid
updates are rejected before saving. API validation errors use a structured
response with an `errors` dictionary.

Roles, write rights, categories, and visibility are stored as settings. This CRUD
feature does not implement permission enforcement, membership management, or
cross-service channel lifecycle events.

## Test coverage

Follow the [repository setup and test instructions](../README.md#database-setup-and-migrations) before running
any tests. SQL Server must have the migration applied and the Compose API must be
running on port 18080.

| Test behaviour | Cases |
| --- | ---: |
| Create a channel and retrieve its values by ID | 1 |
| Update all settings while preserving channel/server identity | 1 |
| Delete, then receive 404 on GET and repeated DELETE | 1 |
| Reject empty/whitespace title, empty type, unsupported type on Create | 4 |
| Reject an empty server ID on Create | 1 |
| Updating a missing ID returns 404 without creating a channel | 1 |
| Reject invalid Update input and preserve existing values | 4 |
| List includes two created records and excludes the deleted record | 1 |
| **Total** | **14** |

The suite checks specific test-record IDs rather than assuming the database is
empty. It uses unique server IDs and attempts cleanup in `finally` blocks. Tests
operate on the existing local database; they do not reset or provision it.

The suite does not automate API-restart persistence, broker messaging, contract
compatibility, or tracing/Prometheus verification. Persistence across an API
restart was checked manually during development. Messaging and contract tests
remain separate Moodle assignment work.

## Manual persistence check

1. POST a temporary channel and save its returned ID.
2. GET it by ID and confirm the values.
3. Run `docker compose restart channel-management-api`.
4. GET the same ID again; expect 200 and the saved values.
5. Delete the temporary channel; expect 204, then 404 when reading it.

SQL data remains in the named volume across API and SQL container restarts.
Removing the volume removes the database; do not use volume removal as a routine
restart step.
