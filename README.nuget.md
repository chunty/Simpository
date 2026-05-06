# Simpository

Generic read/write repository abstractions for Entity Framework Core.

## Key Features

- `IReadRepository<T>` and `IWriteRepository<T>` with full LINQ and async enumeration support
- Auto-registration of repositories from all `DbSet<T>` properties on your `DbContext` — no per-entity wiring
- **Custom repositories** — pass a pre-built queryable to the base constructor to bake in eager loading, ordering, and filtering so every query returns the correct, fully-populated object structure
- Moq helpers for unit testing repositories without hitting a real database

## Installation

```bash
dotnet add package Simpository
```

## Full Documentation

For the complete API reference, custom repository guide, and unit testing examples, see the **[GitHub README](https://github.com/chunty/Simpository#readme)**.
