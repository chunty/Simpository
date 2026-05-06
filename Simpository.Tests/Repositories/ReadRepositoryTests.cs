using Simpository.Exceptions;

namespace Simpository.Tests.Repositories;

public class ReadRepositoryTests
{
    private static async Task<(TestDbContext, TestEntity)> SeedAsync()
    {
        var context = TestDbContext.Create();
        var entity = new TestEntity { Id = 1, Name = "Alice" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return (context, entity);
    }

    [Fact]
    public async Task Find_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedAsync();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Find(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task Find_WhenEntityNotFound_ReturnsNull()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Find(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task Find_WithKeyArray_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedAsync();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Find(new object[] { entity.Id });

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task Find_WithKeyArray_WhenEntityNotFound_ReturnsNull()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Find(new object[] { 999 });

        Assert.Null(result);
    }

    [Fact]
    public async Task FindOrThrow_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedAsync();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.FindOrThrow(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task FindOrThrow_WhenEntityNotFound_ThrowsDataNotFoundException()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<DataNotFoundException<TestEntity>>(() => repo.FindOrThrow(999));
    }

    [Fact]
    public async Task FindOrThrow_WithKeyArray_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedAsync();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.FindOrThrow(new object[] { entity.Id });

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task FindOrThrow_WithKeyArray_WhenEntityNotFound_ThrowsDataNotFoundException()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<DataNotFoundException<TestEntity>>(
            () => repo.FindOrThrow(new object[] { 999 }));
    }

    [Fact]
    public async Task Get_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedAsync();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Get(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task Get_WhenEntityNotFound_ReturnsNull()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Get(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrThrow_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedAsync();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.GetOrThrow(entity.Id);

        Assert.NotNull(result);
        Assert.Equal(entity.Id, result.Id);
    }

    [Fact]
    public async Task GetOrThrow_WhenEntityNotFound_ThrowsDataNotFoundException()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<DataNotFoundException<TestEntity>>(() => repo.GetOrThrow(999));
    }

    [Fact]
    public async Task AsQueryable_AllowsLinqFiltering()
    {
        var context = TestDbContext.Create();
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" },
            new TestEntity { Id = 3, Name = "Charlie" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = repo.Where(e => e.Name.StartsWith("A")).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public async Task ToListAsync_ReturnsAllEntities()
    {
        var context = TestDbContext.Create();
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var result = await repo.ToListAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AsyncEnumeration_ReturnsAllEntities()
    {
        var context = TestDbContext.Create();
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        var results = new List<TestEntity>();
        await foreach (var entity in repo)
            results.Add(entity);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public async Task DefaultBehavior_IsNoTracking()
    {
        var context = TestDbContext.Create();
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        using var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        await repo.ToListAsync();

        Assert.Empty(context.ChangeTracker.Entries<TestEntity>());
    }

    [Fact]
    public async Task Dispose_DisposesDbContext()
    {
        var context = TestDbContext.Create();
        var repo = new ReadRepository<TestEntity, TestDbContext>(context);

        repo.Dispose();

        await Assert.ThrowsAsync<ObjectDisposedException>(
            () => context.TestEntities.ToListAsync());
    }

    // Composite key tests

    private static async Task<(TestDbContext, CompositeKeyEntity)> SeedCompositeAsync()
    {
        var context = TestDbContext.Create();
        var entity = new CompositeKeyEntity { OrderId = 1, LineNumber = 2, Description = "Widget" };
        context.CompositeKeyEntities.Add(entity);
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        return (context, entity);
    }

    [Fact]
    public async Task Get_WithCompositeKeyArray_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedCompositeAsync();
        using var repo = new ReadRepository<CompositeKeyEntity, TestDbContext>(context);

        var result = await repo.Get(new object[] { entity.OrderId, entity.LineNumber });

        Assert.NotNull(result);
        Assert.Equal(entity.OrderId, result.OrderId);
        Assert.Equal(entity.LineNumber, result.LineNumber);
    }

    [Fact]
    public async Task Get_WithCompositeKeyArray_WhenEntityNotFound_ReturnsNull()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<CompositeKeyEntity, TestDbContext>(context);

        var result = await repo.Get(new object[] { 99, 99 });

        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrThrow_WithCompositeKeyArray_WhenEntityExists_ReturnsEntity()
    {
        var (context, entity) = await SeedCompositeAsync();
        using var repo = new ReadRepository<CompositeKeyEntity, TestDbContext>(context);

        var result = await repo.GetOrThrow(new object[] { entity.OrderId, entity.LineNumber });

        Assert.NotNull(result);
        Assert.Equal(entity.OrderId, result.OrderId);
        Assert.Equal(entity.LineNumber, result.LineNumber);
    }

    [Fact]
    public async Task GetOrThrow_WithCompositeKeyArray_WhenEntityNotFound_ThrowsDataNotFoundException()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<CompositeKeyEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<DataNotFoundException<CompositeKeyEntity>>(
            () => repo.GetOrThrow(new object[] { 99, 99 }));
    }

    [Fact]
    public async Task Get_SingleKey_WithCompositeKeyEntity_ThrowsNotSupportedException()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<CompositeKeyEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<NotSupportedException>(() => repo.Get(1));
    }

    [Fact]
    public async Task GetOrThrow_SingleKey_WithCompositeKeyEntity_ThrowsNotSupportedException()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<CompositeKeyEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<NotSupportedException>(() => repo.GetOrThrow(1));
    }

    [Fact]
    public async Task Get_WithCompositeKeyArray_WrongKeyCount_ThrowsArgumentException()
    {
        var context = TestDbContext.Create();
        using var repo = new ReadRepository<CompositeKeyEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<ArgumentException>(() => repo.Get(new object[] { 1 }));
    }
}
