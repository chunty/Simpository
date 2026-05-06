# Simpository

Generic read/write repository abstractions for Entity Framework Core. Provides `IReadRepository<T>` and `IWriteRepository<T>` with full LINQ and async enumeration support, automatic dependency injection registration, and Moq helpers for unit testing.

> **The real power of Simpository is in custom repositories.** By passing a pre-built queryable to the base constructor, you can bake in eager loading, filtering, and ordering so that every query — regardless of how it's called — always returns the correct, fully-populated object structure. [See Extending the repositories ↓](#extending-the-repositories)

## Installation

```
dotnet add package Simpository
```

## Quick Start

### 1. Register repositories

Call one of the extension methods in your `Program.cs` or startup code, passing your `DbContext` type:

```csharp
// Read and write repositories for every DbSet<T> on AppDbContext
builder.Services.AddGenericRepos<AppDbContext>();

// Or register separately
builder.Services.AddGenericReadRepo<AppDbContext>();
builder.Services.AddGenericWriteRepo<AppDbContext>();
```

This inspects all public `DbSet<T>` properties on the context and registers a scoped `IReadRepository<T>` / `IWriteRepository<T>` for each one automatically — no per-entity wiring required.

### 2. Inject and use

```csharp
public class ProductService(IReadRepository<Product> products)
{
    public Task<List<Product>> GetActiveAsync() =>
        products.Where(p => p.IsActive).ToListAsync();

    public Task<Product> GetByIdAsync(int id) =>
        products.GetOrThrow(id);
}
```

```csharp
public class ProductService(IWriteRepository<Product> products)
{
    public Task<Product> CreateAsync(Product product) =>
        products.Add(product);

    public Task DeleteAsync(int id) =>
        products.Delete(id);
}
```

---

## Read vs Write repositories

| | `IReadRepository<T>` | `IWriteRepository<T>` |
|---|---|---|
| Change tracking | **Off** (no-tracking) | **On** |
| LINQ / `ToListAsync` | ✓ | ✓ |
| `async foreach` | ✓ | ✓ |
| Add / Update / Delete | ✗ | ✓ |

Prefer `IReadRepository<T>` wherever mutations are not needed — no-tracking queries are faster and avoid accidental saves.

---

## API Reference

### `IReadRepository<T>`

#### Key lookups

```csharp
// Uses EF Core's FindAsync — checks the change tracker before hitting the database
Task<T?>  Find(object keyValue, CancellationToken ct = default);
Task<T?>  Find(object[] keyValues, CancellationToken ct = default);  // composite keys
Task<T>   FindOrThrow(object keyValue, CancellationToken ct = default);
Task<T>   FindOrThrow(object[] keyValues, CancellationToken ct = default);

// Always queries the database via LINQ
Task<T?>  Get(object key, CancellationToken ct = default);
Task<T>   GetOrThrow(object key, CancellationToken ct = default);
```

`FindOrThrow` / `GetOrThrow` throw `DataNotFoundException<T>` when the entity does not exist.

#### `Find` vs `Get`

- **`Find`** delegates to EF Core's `FindAsync`, which checks the change tracker first — useful in write scenarios where the entity may already be loaded.
- **`Get`** always issues a LINQ query directly against the database — consistent behaviour in read-only scenarios.

#### LINQ & async enumeration

`IReadRepository<T>` implements both `IQueryable<T>` and `IAsyncEnumerable<T>`, so you can use the full EF Core LINQ API:

```csharp
// Filtering, projection, pagination
var page = await repo
    .Where(p => p.IsActive)
    .OrderBy(p => p.Name)
    .Skip(20).Take(10)
    .ToListAsync();

// Async streaming
await foreach (var item in repo)
    Process(item);
```

---

### `IWriteRepository<T>`

Extends `IReadRepository<T>` with mutation operations. All changes are saved immediately via `SaveChangesAsync`.

```csharp
// Add
Task<T>              Add(T entity, CancellationToken ct = default);
Task<IEnumerable<T>> Add(IEnumerable<T> entities, CancellationToken ct = default);

// Update
Task<T>              Update(T entity, CancellationToken ct = default);
Task<IEnumerable<T>> Update(IEnumerable<T> entities, CancellationToken ct = default);

// Delete
Task Delete(T entity,              CancellationToken ct = default);
Task Delete(object key,            CancellationToken ct = default);  // throws if not found
Task Delete(IEnumerable<T> entities, CancellationToken ct = default);
```

Passing an empty collection to `Add` or `Update` returns immediately without touching the database.

---

### `DataNotFoundException<T>`

Thrown by `FindOrThrow`, `GetOrThrow`, and `Delete(object key)` when the requested entity does not exist.

```csharp
try
{
    var product = await repo.GetOrThrow(42);
}
catch (DataNotFoundException<Product> ex)
{
    // ex.Message => "Product not found with key 'Id' using: 42"
}
```

---

## Dependency Injection

| Method | Registers |
|---|---|
| `AddGenericReadRepo<TContext>()` | `IReadRepository<T>` → `ReadRepository<T, TContext>` (scoped) |
| `AddGenericWriteRepo<TContext>()` | `IWriteRepository<T>` → `WriteRepository<T, TContext>` (scoped) |
| `AddGenericRepos<TContext>()` | Both of the above |

Registration is driven by reflection — every public `DbSet<T>` property on `TContext` gets its own repository registration.

---

## Unit Testing

The `Simpository.Mocking` namespace provides Moq extension methods to configure repository mocks with in-memory data, supporting both synchronous LINQ and async enumeration.

```csharp
var mockRepo = new Mock<IProductRepository>();

// With data
mockRepo.SetupData<IProductRepository, Product>(new List<Product>
{
    new() { Id = 1, Name = "Widget" },
    new() { Id = 2, Name = "Gadget" },
});

// Empty
mockRepo.SetupEmptyData<IProductRepository, Product>();
```

Once configured, the mock supports the full LINQ API and `ToListAsync`:

```csharp
var results = await mockRepo.Object
    .Where(p => p.Name.StartsWith("W"))
    .ToListAsync();
```

Methods return the mock instance for chaining:

```csharp
var mockRepo = new Mock<IProductRepository>()
    .SetupData<IProductRepository, Product>(products);
```

---

## Extending the repositories

Inherit from `ReadRepository<T, TContext>` or `WriteRepository<T, TContext>` and pass a custom `IQueryable<T>` to the base constructor to bake in includes, filters, or ordering that apply to every query through that repository.

```csharp
public class ProductRepository(AppDbContext context)
    : ReadRepository<Product, AppDbContext>(
        context,
        context.Products.Include(x => x.Offers).OrderBy(x => x.Price))
{ }
```

Every query through `ProductRepository` automatically gets the eager-loaded offers and price ordering — consumers never need to remember to add them, and every retrieval method returns the same fully-populated object structure:

```csharp
var all      = await repo.ToListAsync();           // List<Product>, each with Offers loaded
var cheap    = await repo.Where(p => p.Price < 10)
                         .ToListAsync();           // still includes Offers, still ordered
var single   = await repo.GetOrThrow(42);          // single Product, Offers loaded
var searched = await repo.FirstOrDefaultAsync(...);// same shape, every time
```

No matter which method a caller uses, they always get back the correct object structure — `Offers` is never `null` because someone forgot to `.Include()` it.

You can combine this with a typed interface to expose domain-specific methods:

```csharp
public interface IProductRepository : IReadRepository<Product>
{
    Task<List<Product>> GetFeaturedAsync();
}

public class ProductRepository(AppDbContext context)
    : ReadRepository<Product, AppDbContext>(
        context,
        context.Products.Include(x => x.Offers).OrderBy(x => x.Price)),
      IProductRepository
{
    public Task<List<Product>> GetFeaturedAsync() =>
        this.Where(p => p.IsFeatured).ToListAsync();
}
```

Register the custom repository alongside or instead of the generic one:

```csharp
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```
