# Simpository

Generic read/write repository abstractions for Entity Framework Core.

## Key Features

- `IReadRepository<T>` and `IWriteRepository<T>` with full LINQ and async enumeration support
- Auto-registration of repositories from all `DbSet<T>` properties on your `DbContext` — no per-entity wiring
- **Custom repositories** — pass a pre-built queryable to the base constructor to bake in eager loading, ordering, and filtering so every query returns the correct, fully-populated object structure
- Moq helpers for unit testing repositories without hitting a real database

## Quick Start

```bash
dotnet add package Simpository
```

```csharp
// Register all repos for every DbSet<T> on AppDbContext
builder.Services.AddGenericRepos<AppDbContext>();

// Inject and use
public class ProductService(IReadRepository<Product> products)
{
    public Task<List<Product>> GetActiveAsync() =>
        products.Where(p => p.IsActive).ToListAsync();
}
```

## Full Documentation

For the complete API reference, custom repository guide, and unit testing examples, see the **[GitHub README](https://github.com/chunty/Simpository#readme)**.
