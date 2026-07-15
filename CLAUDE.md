# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
dotnet build                                                   # build the whole solution
dotnet test                                                    # run all tests (Core, Persistence, Engine)
dotnet test tests/Semogly.Maestro.Persistence.Tests            # run a single test project
dotnet test --filter "FullyQualifiedName~MaestroDbContextTests" # run a single test class
dotnet test --filter "Model_BuildsWithoutErrors"                # run a single test by name
```

Solution file is `Semogly.Maestro.slnx` (the new XML solution format — `dotnet` and `dotnet build`/`test` pick it up automatically without needing `-s`).

Requires the .NET 10 SDK.

## Architecture

Semogly.Maestro is a library for structuring and executing scheduled process pipelines (a workflow engine). It is split into layered projects under `src/`, each with a matching test project under `tests/`:

- **Semogly.Maestro.Core** — plain domain entities, no dependencies on anything else in the solution. This is where the ER model lives (`Entities/`) plus shared enums (`Enums/`).
- **Semogly.Maestro.Abstractions** — depends only on Core. Home for the ports (interfaces) that Engine and Persistence implement against. Holds the activity execution contract (`Activities/`): `IActivityHandler` (implemented by consumers, one per `Activity.Type`), `IActivityHandlerResolver` (maps the `Activity.Type` string to a handler, throws `ActivityHandlerNotFoundException` if unregistered), `ActivityExecutionContext` (the merged `ActivityParam`/`ProcessActivityParam` values passed to a handler at execution time), and `ActivityExecutionContextExtensions` (`GetRequired`/`GetOptional`/`GetBoolean` — every concrete handler reads its params through these). Repository/unit-of-work interfaces for the engine's read/write access to Persistence are deliberately not designed yet — their shape depends on how the scheduler/executor in Engine ends up querying and mutating data, so they'll be added alongside that code instead of speculatively.
- **Semogly.Maestro.Persistence** — depends on Core + Abstractions. `MaestroDbContext` plus one `IEntityTypeConfiguration<T>` per entity under `Configurations/`, applied via `ApplyConfigurationsFromAssembly`. Deliberately provider-agnostic: only `Microsoft.EntityFrameworkCore` + `Microsoft.EntityFrameworkCore.Relational` are referenced (the latter is required for `ToTable`/relational fluent APIs since no provider package is pulled in here). Consumers supply the provider (SQL Server, Postgres, etc.) via `DbContextOptions` when they register the context.
- **Semogly.Maestro.Engine** — depends on Core + Abstractions (not Persistence). `ProcessFlowExecutor` (`IProcessFlowExecutor`) is a pure, persistence-free orchestrator: it takes an already-loaded `ProcessFlow` graph (all navigations populated) and returns a `ProcessFlowExecution` tree with `ProcessExecutions`/`ActivityExecutions` already attached. The caller is responsible for loading the graph and persisting the returned tree (e.g. `dbContext.Add(result); await dbContext.SaveChangesAsync();` — EF's change tracker walks the whole navigation graph from one `Add` call). This is why repository interfaces in Abstractions were skipped: the executor never touches storage.
- **Semogly.Maestro.Activities.\*** — built-in `IActivityHandler` implementations, one project per category so consumers only pull in what they use (mirrors the provider-agnostic decision in Persistence). Each category ships a `*ActivityTypes` constants class (the string keys to register handlers under) and a `*ActivityDefinitions` factory with one method per handler that builds the matching `Activity` + `ActivityParam` entities — use these instead of hand-rolling the param schema, so a handler's expected keys and the seeded `ActivityParam.Key` values can't drift apart.
  - `Semogly.Maestro.Activities.FileSystem` — `MoveFileActivityHandler`/`CopyFileActivityHandler`/`DeleteFileActivityHandler` wrapping `System.IO`.
  - `Semogly.Maestro.Activities.Http` — `HttpRequestActivityHandler` (type `http-request`), constructor-injected `HttpClient` (consumer wires it via `IHttpClientFactory`, e.g. `services.AddHttpClient<HttpRequestActivityHandler>()` — the handler never constructs its own `HttpClient`). Params: `Url` (required), `Method` (optional, default GET), `Body` (optional), `ContentType` (optional, default application/json), `Headers` (optional, one `"Key: Value"` per line — the flat `ActivityParam` model can't carry a variable-length header set, so they're packed into one string param and parsed). Any non-2xx response throws `HttpRequestException` with the status and response body in the message; on success the response body is returned as the activity's `Output`.
  - New categories (email, etc.) get their own project the same way when there's a concrete need for them.

### Domain model

Three tiers, each with a definition/template level and an ordered link level, plus a parallel execution-tracking level:

```
ProcessFlow  --ProcessFlowProcess(Order)-->  Process  --ProcessActivity(Order)-->  Activity --> ActivityParam
     |                    |                                    |
ProcessFlowExecution --> ProcessExecution              -->  ActivityExecution
```

- `ProcessFlow` is the top-level, cron-scheduled unit (`CronParam`). It runs an ordered set of `Process`es via the `ProcessFlowProcess` link table, which can override the flow's cron per step.
- `Process` runs an ordered set of `Activity` instances via `ProcessActivity`.
- `Activity.Type` is a string key, not an enum — it's matched at runtime against a registered activity handler (defined in Abstractions), so new activity kinds don't require a schema/enum change.
- Parameters are split into two levels: `ActivityParam` declares an activity's parameter schema (key, required, default); `ProcessActivityParam` supplies the concrete value for one `ProcessActivity` instance.
- Every definition/link level has a matching `*Execution` entity (`ProcessFlowExecution`, `ProcessExecution`, `ActivityExecution`) that mirrors its structure and adds `Status` (shared `ExecutionStatus` enum: Pending/Running/Completed/Failed/Cancelled), `StartedAt`, `FinishedAt`, `Error`. `ActivityExecution` additionally tracks `RetryCount` and `Output` (whatever `IActivityHandler.ExecuteAsync` returns on success, e.g. an HTTP response body — `null` if the handler produced nothing).

### Failure and retry behavior (`ProcessFlowExecutor`)

- `Activity.MaxRetryCount` — extra attempts the executor makes immediately (no backoff, no delay) after a handler throws, before marking the `ActivityExecution` `Failed`. 0 = no retry. A thrown `ActivityHandlerNotFoundException` is never retried, since retrying can't make a missing registration appear.
- `Process.ContinueOnActivityFailure` — if `false` (default), one failed `Activity` skips the rest of that `Process`'s activities (they get no `ActivityExecution` at all — not even `Cancelled`). If `true`, the executor runs every activity regardless of earlier failures.
- `ProcessFlow.ContinueOnProcessFailure` — same idea one level up: if `false` (default), one failed `Process` skips the remaining `Process`es in the flow.
- Missing a `Required` `ActivityParam` (no `ProcessActivityParam` override and no `DefaultValue`) fails the `ActivityExecution` before `IActivityHandler.ExecuteAsync` is ever called, and is not retried (retrying can't fix a missing value either).
- These two `Continue*` flags are independent and compose: both `false` behaves like "abort everything on first failure"; `Process.ContinueOnActivityFailure=false` + `ProcessFlow.ContinueOnProcessFailure=true` isolates a failure to just its own process; both `true` is fully best-effort.

### Conventions established so far

- All entity IDs are `Guid`, generated client-side in `Entity.Id` (`= Guid.NewGuid()`), not database-generated — avoids a round-trip before an entity can be linked to children in the same unit of work.
- FK delete behavior follows one rule: relationships within the same execution/definition branch that "belong" to their parent (e.g. `ProcessFlow -> ProcessFlowProcess`, `Activity -> ActivityParam`, `ProcessExecution -> ActivityExecution`) cascade; relationships that point at a reusable/shared definition (e.g. `ProcessFlowProcess -> Process`, `ProcessActivity -> Activity`, any `*Execution -> ` its definition) are `Restrict`, so deleting a shared `Process`/`Activity`/definition while still referenced fails loudly instead of silently orphaning history.
- Each relationship is configured from exactly one side (the "one" side of a one-to-many) to avoid duplicate/conflicting fluent configuration between an entity's own `IEntityTypeConfiguration` and its related entities'.
- `ExecutionStatus` is persisted as a string (`HasConversion<string>()`), not the underlying int, so the enum can be reordered/extended without corrupting stored data.
