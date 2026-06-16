# Refactor: Clean Architecture + Vertical Slices Hybrid

## Objective

Refactor the `clean-architecture` solution from a **horizontal-layers-first** structure to a
**vertical-slices-first** structure, where each feature slice owns every artefact it changes
together — command, handler, validator, endpoint, request/response contract — while preserving
the domain layer and SharedKernel as genuinely shared, cross-cutting concerns.

The guiding principle throughout is:

> **High cohesion** — things that change together live together.  
> **Low coupling** — slices never import from each other; shared code is earned, not assumed.

---

## Current Structure (what exists today)

```
clean-architecture.sln
├── SharedKernel/                    ← Result, Error, Entity, ValueObject, IDomainEvent
├── clean-architecture.contracts/   ← NoteResponse, CreateNoteRequest, UpdateNoteRequest
├── clean-architecture.domain/      ← Note, NoteTitle, NoteContent, NoteErrors, DomainEvents
├── clean-architecture.application/
│   ├── Abstractions/
│   │   ├── Data/IApplicationDbContext.cs
│   │   ├── Messaging/              ← ICommand, IQuery, ICommandHandler, IQueryHandler
│   │   └── Behaviours/            ← ValidationDecorator, LoggingDecorator
│   ├── Notes/
│   │   ├── Create/                ← CreateNoteCommand, CreateNoteCommandHandler
│   │   ├── Update/                ← UpdateNoteCommand, UpdateNoteCommandHandler
│   │   ├── Delete/                ← DeleteNoteCommand, DeleteNoteCommandHandler
│   │   ├── Get/                   ← GetNoteQuery, GetNoteQueryHandler
│   │   └── GetById/               ← GetNoteByIdQuery, GetNoteByIdQueryHandler
│   └── DependencyInjection.cs
├── clean-architecture.infrastructure/
│   ├── Database/                  ← ApplicationDbContext, Schemas, DatabaseProvider
│   ├── DomainEvents/              ← DomainEventsDispatcher
│   ├── Time/                      ← DateTimeProvider
│   └── DependencyInjection.cs
└── clean-architecture.WebApi/
    ├── Endpoints/Notes/           ← Create.cs, Update.cs, Delete.cs, Get.cs, GetById.cs
    ├── Extensions/
    ├── Infrastructure/            ← GlobalExceptionHandler, CustomResults
    └── Middleware/
```

---

## Target Structure

```
clean-architecture.sln
├── SharedKernel/                          ← UNCHANGED — Result, Error, Entity, ValueObject, IDomainEvent
├── clean-architecture.domain/            ← UNCHANGED — Note aggregate, value objects, errors, domain events
├── clean-architecture.application/
│   ├── Abstractions/
│   │   ├── Data/IApplicationDbContext.cs ← UNCHANGED
│   │   ├── Messaging/                    ← UNCHANGED — ICommand, IQuery, ICommandHandler, IQueryHandler
│   │   └── Behaviours/                   ← UNCHANGED — ValidationDecorator, LoggingDecorator
│   ├── Features/
│   │   └── Notes/
│   │       ├── Create/
│   │       │   ├── CreateNoteCommand.cs
│   │       │   ├── CreateNoteCommandHandler.cs
│   │       │   ├── CreateNoteValidator.cs       ← move or create here if exists
│   │       │   ├── CreateNoteRequest.cs         ← moved from contracts project
│   │       │   ├── CreateNoteResponse.cs        ← renamed from NoteResponse (scoped to slice)
│   │       │   └── CreateNoteEndpoint.cs        ← moved from WebApi/Endpoints/Notes/Create.cs
│   │       ├── Update/
│   │       │   ├── UpdateNoteCommand.cs
│   │       │   ├── UpdateNoteCommandHandler.cs
│   │       │   ├── UpdateNoteValidator.cs
│   │       │   ├── UpdateNoteRequest.cs
│   │       │   ├── UpdateNoteResponse.cs
│   │       │   └── UpdateNoteEndpoint.cs
│   │       ├── Delete/
│   │       │   ├── DeleteNoteCommand.cs
│   │       │   ├── DeleteNoteCommandHandler.cs
│   │       │   └── DeleteNoteEndpoint.cs
│   │       ├── Get/
│   │       │   ├── GetNotesQuery.cs
│   │       │   ├── GetNotesQueryHandler.cs
│   │       │   ├── GetNotesResponse.cs
│   │       │   └── GetNotesEndpoint.cs
│   │       └── GetById/
│   │           ├── GetNoteByIdQuery.cs
│   │           ├── GetNoteByIdQueryHandler.cs
│   │           ├── GetNoteByIdResponse.cs
│   │           └── GetNoteByIdEndpoint.cs
│   └── DependencyInjection.cs
├── clean-architecture.infrastructure/   ← UNCHANGED internally
└── clean-architecture.WebApi/
    ├── Infrastructure/                   ← UNCHANGED — GlobalExceptionHandler, CustomResults
    ├── Extensions/                       ← UNCHANGED — EndpointExtensions, MiddlewareExtensions, etc.
    ├── Middleware/                       ← UNCHANGED
    └── DependencyInjection.cs           ← UNCHANGED
```

---

## Step-by-Step Implementation

### Step 1 — Move contracts into their owning slices

For each operation:

1. Move `CreateNoteRequest.cs` → `application/Features/Notes/Create/CreateNoteRequest.cs`  
   Update its namespace to `clean_architecture.application.Features.Notes.Create`.

2. Move `UpdateNoteRequest.cs` → `application/Features/Notes/Update/UpdateNoteRequest.cs`  
   Update namespace accordingly.

3. `NoteResponse` is currently shared across Create, Update, GetById, and Get handlers. This is
   the central cohesion decision: **each slice gets its own response type** named after itself
   (`CreateNoteResponse`, `UpdateNoteResponse`, `GetNoteByIdResponse`, `GetNotesResponse`).  
   All carry the same fields as the current `NoteResponse` record — do not drop or rename fields.

4. After all consumers are migrated, **delete** `clean-architecture.contracts/`.  
   Remove its `<ProjectReference>` from all `.csproj` files.

**Coupling rule:** No slice's response type may be imported by another slice. If two slices
currently share a type purely for convenience, that is incidental coupling — break it.

---

### Step 2 — Relocate commands and queries

Move each command/query file into its slice folder under `Features/Notes/<Operation>/`.
Update namespaces. No logic changes — only file location and namespace.

```
application/Notes/Create/CreateNoteCommand.cs
  → application/Features/Notes/Create/CreateNoteCommand.cs
  namespace: clean_architecture.application.Features.Notes.Create
```

Repeat for Update, Delete, Get, GetById.

---

### Step 3 — Relocate handlers

Move each handler into its slice folder. Update namespaces and `using` directives to reference
the new response types created in Step 1.

Critical changes in handlers:
- Replace `NoteResponse` return types with the slice-local response type
  (`CreateNoteResponse`, `UpdateNoteResponse`, etc.)
- Remove `using clean_architecture.contracts.Notes;`
- Add `using clean_architecture.application.Features.Notes.<Operation>;`

Do **not** change any handler logic, EF Core queries, or result construction — only the
type references that flow across the old layer boundary.

---

### Step 4 — Move endpoints into slices

Each endpoint file in `WebApi/Endpoints/Notes/` moves to the corresponding slice folder in
`application/Features/Notes/<Operation>/`.

```
WebApi/Endpoints/Notes/Create.cs
  → application/Features/Notes/Create/CreateNoteEndpoint.cs
  namespace: clean_architecture.application.Features.Notes.Create
```

Rules for each endpoint file:
- Rename the class from `Create` to `CreateNoteEndpoint`, `UpdateNoteEndpoint`, etc.
- Update `using` directives — remove `clean_architecture.contracts.Notes`, add the
  slice-local namespace.
- The class must still implement `IEndpoint` from `clean_architecture.WebApi.Endpoints`.
- Do not change the route strings, HTTP methods, tags, `.WithName()`, `.WithSummary()`,
  or `.WithDescription()` values.

**Project reference note:** `application.csproj` does not currently reference
`clean_architecture.WebApi`. The `IEndpoint` interface must therefore be moved or extracted
so it is reachable from the application layer.

Extract `IEndpoint` from `WebApi/Endpoints/IEndpoint.cs` into
`application/Abstractions/Endpoints/IEndpoint.cs` with namespace
`clean_architecture.application.Abstractions.Endpoints`.

Update:
- `WebApi/Endpoints/IEndpoint.cs` → delete, or make it a `using` alias pointing to the
  new location if any WebApi-only code still references the old path.
- `EndpointExtensions.cs` → update the `using` to the new namespace.
- All endpoint classes → update the `using` to the new namespace.

Add a `<ProjectReference>` from `clean-architecture.WebApi.csproj` to
`clean-architecture.application.csproj` if it does not already exist (it should, given
the current DI wiring).

---

### Step 5 — Fix the known bugs in the template

These bugs exist in the current codebase and must be corrected as part of this refactor.
They are not pre-existing acceptable state.

**Bug 1 — `Get.cs` routes to the wrong handler**

Current `WebApi/Endpoints/Notes/Get.cs`:
```csharp
app.MapGet("/notes/{id:guid}", async (Guid id, IQueryHandler<GetNoteQuery, List<NoteResponse>>
handler, CancellationToken cancellation) =>
{
    var query = new GetNoteQuery();
    var results = await handler.Handle(query, cancellation);
    ...
})
```

This registers a `GET /notes/{id:guid}` route that resolves `GetNoteQuery` (the list query)
instead of `GetNoteByIdQuery`. It also conflicts with `GetById.cs` on the same route.

Fix:
- Change the route in `GetNotesEndpoint.cs` to `GET /notes` (no `{id:guid}` segment).
- Remove the unused `Guid id` parameter from the handler lambda.
- The `GetById` endpoint correctly stays at `GET /notes/{id:guid}`.

**Bug 2 — `ApplicationDbContext.Notes` is a throwing stub**

Current `ApplicationDbContext.cs`:
```csharp
public DbSet<Note> Notes { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
```

Fix:
```csharp
public DbSet<Note> Notes { get; set; } = null!;
```

---

### Step 6 — Update `DependencyInjection.cs` in application layer

The scanner in `application/DependencyInjection.cs` currently scans
`typeof(DependencyInjection).Assembly`. Since all handlers now live in the same assembly
(just under a different sub-namespace), no assembly scanning changes are needed.

However, endpoint registration currently lives in `WebApi/DependencyInjection.cs` or
`WebApi/Extensions/EndpointExtensions.cs`. Because endpoint classes have moved into the
application assembly, update `AddEndpoints(assembly)` in the WebApi startup to pass
**both** assemblies:

```csharp
services.AddEndpoints(typeof(clean_architecture.application.DependencyInjection).Assembly);
services.AddEndpoints(typeof(clean_architecture.WebApi.DependencyInjection).Assembly);
```

Or consolidate into a single call if `EndpointExtensions.AddEndpoints` is updated to accept
`params Assembly[]`.

---

### Step 7 — Delete the contracts project

Once all usages of types from `clean-architecture.contracts` have been migrated:

1. Verify with `dotnet build` — zero references to `clean_architecture.contracts` should remain.
2. Delete the `clean-architecture.contracts/` directory.
3. Remove the `<Project>` entry from `clean-architecture.slnx`.
4. Remove all `<ProjectReference Include="...\clean-architecture.contracts\...">` lines from
   consuming `.csproj` files.

---

### Step 8 — Verify with build

Run:
```bash
dotnet build clean-architecture.slnx
```

**Acceptance criterion: zero errors, zero warnings about missing references.**

If there are residual `using clean_architecture.contracts.Notes;` directives, the build will
catch them. Resolve each by updating to the slice-local namespace.

---

## Cohesion Rules (enforce throughout)

| What belongs in a slice | What does NOT belong in a slice |
|---|---|
| The command or query for that operation | Domain logic (keep in `domain/`) |
| The handler for that command or query | EF Core DbContext configuration |
| The FluentValidation validator for that command | Cross-cutting behaviours (logging, validation decorators) |
| The HTTP endpoint (minimal API) for that operation | Infrastructure (DbContext, DomainEventsDispatcher) |
| The request DTO for that operation | SharedKernel types (Result, Error, Entity) |
| The response DTO for that operation | Another slice's types |

A slice is allowed to **use** SharedKernel types, domain types, and `IApplicationDbContext`.
It must never **import** a type from a sibling slice.

---

## Coupling Rules (enforce throughout)

1. **Domain layer has no outward dependencies.** It must not reference any slice, application
   abstraction, or infrastructure type. This is already true — do not break it.

2. **SharedKernel has no outward dependencies.** Already true — preserve it.

3. **Slices depend inward only.** A slice may depend on:
   - `SharedKernel` (Result, Error, Entity, etc.)
   - `clean_architecture.domain` (aggregates, value objects, domain errors)
   - `clean_architecture.application.Abstractions` (IApplicationDbContext, ICommand, IQuery,
     ICommandHandler, IQueryHandler, IEndpoint)
   - Nothing else.

4. **Infrastructure depends on application abstractions, not on slices.** `ApplicationDbContext`
   implements `IApplicationDbContext` — it must not reference any slice type directly.

5. **No circular references.** The dependency graph is:
   ```
   SharedKernel ← domain ← application.Abstractions ← application.Features ← infrastructure
                                                     ↑                        ↑
                                                  WebApi ←────────────────────┘
   ```

---

## What NOT to Do

- **Do not** merge slices. Each operation (Create, Update, Delete, Get, GetById) stays its own
  folder. A `Notes/` god-folder with all files dumped in it defeats the purpose.

- **Do not** create a shared `Notes/Shared/NoteResponse.cs` that all Note slices import.
  That recreates the coupling you are removing from the contracts project. Each slice owns
  its own response type, even if the fields are identical today.

- **Do not** move `ApplicationDbContext`, `DomainEventsDispatcher`, or `DateTimeProvider` into
  any slice. They are infrastructure and remain in `clean-architecture.infrastructure/`.

- **Do not** move `Note`, `NoteTitle`, `NoteContent`, or `NoteErrors` into any slice.
  The domain layer is intentionally separate.

- **Do not** change any handler's business logic, EF Core query, or result construction.
  This is a structural refactor only.

- **Do not** change route strings, HTTP method verbs, or OpenAPI metadata on endpoints.

- **Do not** introduce MediatR, a mediator pattern, or any new NuGet packages. The existing
  `ICommandHandler<,>` / `IQueryHandler<,>` dispatch pattern is sufficient and must be kept.

- **Do not** suppress build errors with `#pragma warning disable` or `<NoWarn>`. Every
  compile error must be properly resolved.

- **Do not** leave the two known bugs (Get route collision, ApplicationDbContext.Notes stub)
  unfixed. They are mandatory corrections in this changeset.

---

## Definition of Done

- [ ] `dotnet build clean-architecture.slnx` exits with code 0, zero errors.
- [ ] `clean-architecture.contracts/` project no longer exists.
- [ ] No file outside a `Features/Notes/<Operation>/` folder contains a `using` referencing
      `clean_architecture.contracts.Notes`.
- [ ] Each slice folder contains exactly: command or query, handler, request DTO (if applicable),
      response DTO, endpoint, and validator (if applicable). No more, no less.
- [ ] `GET /notes` returns the list. `GET /notes/{id:guid}` returns a single note by ID.
      No route collision between `Get` and `GetById`.
- [ ] `ApplicationDbContext.Notes` is an auto-property (`{ get; set; } = null!;`).
- [ ] No slice imports a type from a sibling slice.
- [ ] The domain layer, SharedKernel, infrastructure internals, and
      cross-cutting behaviours are structurally unchanged.
