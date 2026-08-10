# clean-architecture

> **The template source for [CleanArchitectureGenerator](https://www.nuget.org/packages/CleanArchitectureGenerator).**

This repository is the reference implementation that the `CleanArchitectureGenerator` CLI reads, parameterises, and scaffolds into new projects. Every file here represents the output a developer receives when they run `cleanarch new`. If you want to understand what a generated solution looks like, or improve what gets generated, this is the right place.

---

## What This Repo Is

When you run the generator:

```bash
dotnet tool install --global CleanArchitectureGenerator

# Non-interactive (full flow):
cleanarch new MyApp -d postgres -o ./MyApp
cleanarch new MyApp -d sqlserver -o ./MyApp
```

the CLI copies and tokenises the files in this repository — substituting project names, namespaces, and database-provider-specific code — to produce a ready-to-build solution. Changes merged here flow directly into every project scaffolded by future versions of the generator.

---

## Technology Stack

| Layer | Technology |
| --- | --- |
| Runtime | .NET 10 |
| Web Framework | ASP.NET Core (Minimal APIs) |
| ORM | Entity Framework Core 10 |
| Databases | SQL Server, PostgreSQL |
| Validation | FluentValidation 12 |
| Logging | Serilog (console + SQL Server sink) |
| Resilience | Polly (retry, timeout) |
| Caching | HybridCache (in-process, opt-in via `ICachedQuery`) |
| DI Extensions | Scrutor |
| API Docs | Scalar (OpenAPI) |
| Testing | xUnit, Moq, FluentAssertions, Bogus |
| Orchestration | .NET Aspire (AppHost starts the API and the frontend together) |
| Frontend | Next.js (App Router), React 19, TypeScript, TailwindCSS 4 |

---

## Project Architecture

The solution enforces a strict inward dependency rule across five layers:

```text
SharedKernel
    └── Domain
            └── Application
                    ├── Infrastructure   (outbound — database, services)
                    └── WebApi           (inbound — HTTP endpoints)
```

| Layer | Responsibility |
| --- | --- |
| **SharedKernel** | Base types: `Entity`, `ValueObject`, `Result<T>`, `Error`, domain event interfaces |
| **Domain** | Aggregates, value objects, domain events, and domain errors — zero framework dependencies |
| **Application** | Use cases as CQRS command/query handlers; validation, resilience, caching, and logging decorators |
| **Infrastructure** | EF Core `DbContext`, migrations, domain event dispatcher, health checks, `DateTimeProvider` |
| **WebApi** | Minimal API endpoints, global exception handler, problem-details mapping, Scalar UI |

### Key Patterns

- **CQRS** — Commands and queries implemented via `ICommandHandler` / `IQueryHandler` with Scrutor decorator chaining
- **Domain Events** — `NoteCreatedDomainEvent`, `NoteUpdatedDomainEvent`, `NoteDeletedDomainEvent` dispatched after `SaveChanges`
- **Result / Railway Pattern** — `Result<T>` and `Error` replace exception-driven flow for domain outcomes
- **Decorator Pipeline** — `ValidationDecorator` → `ResilienceDecorator` → `CachingDecorator` → `LoggingDecorator` → handler, registered transparently via Scrutor
- **Resilience** — Query handlers are wrapped in a named Polly pipeline (3 retries, exponential backoff, 10s timeout) for transient failures; commands are not retried to avoid re-running non-idempotent writes. The PostgreSQL and SQL Server providers also enable EF Core's connection-level retry-on-failure
- **Hybrid Caching** — Queries implementing `ICachedQuery` (cache key, expiration, tags) are cached via `HybridCache`; Notes command handlers invalidate cached entries by tag on create/update/delete
- **Value Objects** — `NoteTitle` and `NoteContent` enforce invariants at the type level

---

## Running This Repo Locally

This repo exists primarily as a template source, but it builds and runs as a standalone API — useful for verifying changes before they flow into the generator.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server **or** PostgreSQL instance

### Configure the database

Open `clean-architecture.WebApi/appsettings.Development.json` and add your connection string:

```json
{
  "ConnectionStrings": {
    "LocalDb": "Server=localhost;Database=CleanArch;Trusted_Connection=True;"
  },
  "Database": {
    "Provider": "SqlServer"
  }
}
```

Set `"Provider"` to `"PostgreSql"` and supply a Postgres connection string to use that provider instead.

### Apply migrations

```bash
dotnet ef database update --project clean-architecture.infrastructure --startup-project clean-architecture.WebApi
```

### Run the API only

```bash
dotnet run --project clean-architecture.WebApi
```

The API starts on `http://localhost:5286`. Open `http://localhost:5286/scalar` for the interactive API reference (development only).

### Run the API and frontend together (via Aspire)

```bash
cd clean-architecture.Web && npm install && cd ..   # one-time
dotnet run --project clean-architecture.AppHost
```

This opens the Aspire dashboard and starts both the `clean-architecture-webapi` and `clean-architecture-web` (Next.js) resources, wiring the frontend's API base URL automatically. The Next.js app is available at `http://localhost:3000`.

To run the frontend standalone (outside Aspire), copy `clean-architecture.Web/.env.local.example` to `.env.local` and run `npm run dev` from `clean-architecture.Web/`.

---

## Project Structure

```text
clean-architecture/
├── SharedKernel/                      # Base classes, Result pattern, domain event interfaces
├── clean-architecture.domain/         # Aggregates, value objects, domain events, errors
├── clean-architecture.application/    # CQRS handlers, validation, abstractions
├── clean-architecture.infrastructure/ # EF Core, migrations, event dispatcher
├── clean-architecture.WebApi/         # Minimal API endpoints, middleware, startup
├── clean-architecture.Web/            # Next.js frontend (App Router)
└── clean-architecture.AppHost/        # .NET Aspire orchestration (starts API + frontend)
```

---

## Contributing

Contributions are very welcome. Improvements here improve every project the generator scaffolds.

### What to contribute

- **Bug fixes** in generated code (handler logic, endpoint wiring, EF Core configuration)
- **Pattern improvements** — better ways to express CQRS, the Result pattern, or domain events
- **New cross-cutting concerns** — additional decorators, middleware, or health check configurations worth including in a generated baseline
- **Documentation** — clearer comments, XML doc improvements, or README updates

### How to contribute

1. **Fork** the repository and create a branch: `git checkout -b fix/short-description` or `git checkout -b feature/my-feature`
2. **Keep changes focused** — one fix or feature per PR makes review faster and keeps history readable
3. **Verify the build passes** before opening a PR: `dotnet build && dotnet test`
4. **Open a pull request** against `main` with a clear description of what changed and why

If you're unsure whether a change belongs here, open an issue first. Some improvements are better suited to the generator itself rather than the template.

### Coding standards

- **Value objects over primitives** — wrap domain concepts in types (`NoteTitle`, `NoteContent`) rather than raw strings
- **Result pattern, not exceptions** — return `Result<T>` / `Error` for expected failure paths
- **Thin endpoints** — endpoints dispatch to handlers and map results; no business logic in `IEndpoint` implementations
- **No `DateTime.Now`** — use `IDateTimeProvider` so tests remain deterministic
- **Immutable records** — prefer `record` types for commands, queries, and DTOs


---

## License

MIT — see [LICENSE.txt](LICENSE.txt) for details.