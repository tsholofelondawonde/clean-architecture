# clean-architecture

A production-ready reference implementation of **Clean Architecture** in .NET 10, demonstrating Domain-Driven Design (DDD), CQRS, the Result pattern, and Minimal APIs. Built around a Notes Management API as the domain showcase.

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
| DI Extensions | Scrutor |
| API Docs | Scalar (OpenAPI) |
| Testing | xUnit, Moq, FluentAssertions, Bogus |

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

### Layers

| Layer | Responsibility |
| --- | --- |
| **SharedKernel** | Base types: `Entity`, `ValueObject`, `Result<T>`, `Error`, domain event interfaces |
| **Domain** | Aggregates, value objects, domain events, and domain errors — zero framework dependencies |
| **Application** | Use cases as CQRS command/query handlers; validation and logging decorators |
| **Infrastructure** | EF Core `DbContext`, migrations, domain event dispatcher, health checks, `DateTimeProvider` |
| **WebApi** | Minimal API endpoints, global exception handler, problem-details mapping, Scalar UI |

### Key Patterns

- **CQRS** — Commands and queries implemented via `ICommandHandler` / `IQueryHandler` with Scrutor decorator chaining
- **Domain Events** — `NoteCreatedDomainEvent`, `NoteUpdatedDomainEvent`, `NoteDeletedDomainEvent` dispatched after `SaveChanges`
- **Result / Railway Pattern** — `Result<T>` and `Error` replace exception-driven flow for domain outcomes
- **Decorator Pipeline** — `ValidationDecorator` → `LoggingDecorator` → handler, registered transparently via Scrutor
- **Value Objects** — `NoteTitle` and `NoteContent` enforce invariants at the type level

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server **or** PostgreSQL instance

### Clone

```bash
git clone https://github.com/<your-org>/clean-architecture.git
cd clean-architecture
```

### Configure the database

Open `clean-architecture.WebApi/appsettings.json` and set your connection string:

```json
{
  "ConnectionStrings": {
    "Database": "Server=localhost;Database=CleanArch;Trusted_Connection=True;"
  }
}
```

Switch to PostgreSQL by changing the provider key (see `Infrastructure/DependencyInjection.cs`).

### Apply migrations

```bash
dotnet ef database update --project clean-architecture.infrastructure --startup-project clean-architecture.WebApi
```

### Run

```bash
dotnet run --project clean-architecture.WebApi
```

The API starts on `https://localhost:5286`. Open `https://localhost:5286/scalar` for the interactive API reference.

---

## Project Structure

```text
clean-architecture/
├── SharedKernel/                   # Base classes, Result pattern, domain event interfaces
├── clean-architecture.domain/      # Aggregates, value objects, domain events, errors
├── clean-architecture.application/ # CQRS handlers, validation, abstractions
├── clean-architecture.contracts/   # Request/response DTOs (no business logic)
├── clean-architecture.infrastructure/ # EF Core, migrations, event dispatcher
├── clean-architecture.WebApi/      # Minimal API endpoints, middleware, startup
└── clean-architecture.client/      # (Future) front-end client
```

---

## Key Features

- **Notes CRUD** — create, read, update, and soft-delete notes via a REST API
- **Soft Delete** — `IsDeleted` flag preserves data without hard removal
- **Value Object Validation** — `NoteTitle` (≤ 100 chars) and `NoteContent` (≤ 1 000 chars) enforced in the domain
- **Decorator Pipeline** — FluentValidation and structured logging applied cross-cutting without polluting handlers
- **Multi-Database Support** — SQL Server and PostgreSQL, configurable at startup
- **Health Checks** — `/health` endpoint reports database connectivity
- **Global Exception Handler** — maps unhandled exceptions to RFC 9457 problem details
- **Structured Logging** — Serilog with request-context enrichment and SQL Server persistence
- **Scalar API Docs** — interactive OpenAPI reference at `/scalar` (development only)
- **Centralized Package Versioning** — `Directory.Packages.props` keeps NuGet versions consistent

---

## Development Workflow

### Branching

- `master` — stable, production-ready
- `feature/<name>` — new features
- `fix/<name>` — bug fixes

Create a pull request against `master` when ready.

### Adding a new use case

1. **Domain** — add any new value objects or domain events to `clean-architecture.domain`
2. **Application** — create a `Command` / `Query` record and a corresponding `Handler` in `clean-architecture.application/Notes/<Operation>/`
3. **Contracts** — add request/response DTOs in `clean-architecture.contracts` if needed
4. **WebApi** — register a new `IEndpoint` implementation in `clean-architecture.WebApi/Endpoints/`

Use `CreateNoteCommandHandler` as a reference for commands and `GetNoteQueryHandler` for queries.

---

## Coding Standards

- **Value objects over primitives** — wrap domain concepts (`NoteTitle`, `NoteContent`) instead of using raw strings
- **Result pattern, not exceptions** — return `Result<T>` / `Error` for expected failure paths; reserve exceptions for truly exceptional conditions
- **Thin endpoints** — endpoints dispatch to handlers and map results; no business logic in `IEndpoint` implementations
- **Pure handlers** — command/query handlers depend only on abstractions (`IApplicationDbContext`, `IDateTimeProvider`)
- **Immutable records** — prefer C# `record` types for commands, queries, and DTOs
- **No `DateTime.Now`** — use `IDateTimeProvider` for all time access so tests remain deterministic

---

## Testing

The test suite uses **xUnit** with the following supporting libraries:

| Library | Purpose |
| --- | --- |
| Moq | Mocking dependencies |
| FluentAssertions | Readable assertions |
| Bogus | Fake test data generation |
| SQLite (in-memory) | Lightweight integration testing against a real EF Core context |

### Running tests

```bash
dotnet test
```

---

## Contributing

Contributions are welcome! Here's how to get started:

1. **Fork** the repository and create a feature branch (`git checkout -b feature/my-feature`)
2. **Follow the coding standards** described above — look at existing handlers and value objects as examples
3. **Write tests** for new use cases (handlers, value objects, domain logic)
4. **Open a pull request** with a clear description of the change and why it's needed

Please keep pull requests focused. One feature or fix per PR makes review faster and keeps history clean.

If you find a bug or have a feature request, open an issue first so it can be discussed before work begins.

---

## License

This project is licensed under the **MIT License** — see [LICENSE.txt](LICENSE.txt) for details.
