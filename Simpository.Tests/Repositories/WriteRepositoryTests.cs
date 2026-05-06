using Simpository.Exceptions;

namespace Simpository.Tests.Repositories;

public class WriteRepositoryTests
{
    [Fact]
    public async Task Add_SingleEntity_AddsAndReturnsEntity()
    {
        var context = TestDbContext.Create();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Add(new TestEntity { Id = 1, Name = "Alice" });

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal(1, await context.TestEntities.CountAsync());
    }

    [Fact]
    public async Task Add_MultipleEntities_AddsAndReturnsAll()
    {
        var context = TestDbContext.Create();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Add(new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" }
        });

        Assert.Equal(2, result.Count());
        Assert.Equal(2, await context.TestEntities.CountAsync());
    }

    [Fact]
    public async Task Add_EmptyCollection_ReturnsEmptyWithoutSaving()
    {
        var context = TestDbContext.Create();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Add(new List<TestEntity>());

        Assert.Empty(result);
        Assert.Equal(0, await context.TestEntities.CountAsync());
    }

    [Fact]
    public async Task Update_SingleEntity_UpdatesAndReturnsEntity()
    {
        var context = TestDbContext.Create();
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
        await context.SaveChangesAsync();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        var entity = await context.TestEntities.FindAsync(1);
        entity!.Name = "Updated";
        var result = await repo.Update(entity);

        Assert.Equal("Updated", result.Name);
        Assert.Equal("Updated", (await context.TestEntities.FindAsync(1))!.Name);
    }

    [Fact]
    public async Task Update_MultipleEntities_UpdatesAndReturnsAll()
    {
        var context = TestDbContext.Create();
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" });
        await context.SaveChangesAsync();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        var entities = await context.TestEntities.ToListAsync();
        foreach (var e in entities) e.Name = "Updated " + e.Name;
        var result = (await repo.Update(entities)).ToList();

        Assert.Equal(2, result.Count);
        Assert.All(result, e => Assert.StartsWith("Updated", e.Name));
    }

    [Fact]
    public async Task Update_EmptyCollection_ReturnsEmpty()
    {
        var context = TestDbContext.Create();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        var result = await repo.Update(new List<TestEntity>());

        Assert.Empty(result);
    }

    [Fact]
    public async Task Delete_SingleEntity_RemovesFromStore()
    {
        var context = TestDbContext.Create();
        var entity = new TestEntity { Id = 1, Name = "Alice" };
        context.TestEntities.Add(entity);
        await context.SaveChangesAsync();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        await repo.Delete(entity);

        Assert.Equal(0, await context.TestEntities.CountAsync());
    }

    [Fact]
    public async Task Delete_ByKey_FindsAndRemovesEntity()
    {
        var context = TestDbContext.Create();
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        await repo.Delete(1);

        Assert.Equal(0, await context.TestEntities.CountAsync());
    }

    [Fact]
    public async Task Delete_ByKey_WhenNotFound_ThrowsDataNotFoundException()
    {
        var context = TestDbContext.Create();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        await Assert.ThrowsAsync<DataNotFoundException<TestEntity>>(() => repo.Delete(999));
    }

    [Fact]
    public async Task Delete_MultipleEntities_RemovesAll()
    {
        var context = TestDbContext.Create();
        context.TestEntities.AddRange(
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" });
        await context.SaveChangesAsync();
        using var repo = new WriteRepository<TestEntity, TestDbContext>(context);

        var entities = await context.TestEntities.ToListAsync();
        await repo.Delete(entities);

        Assert.Equal(0, await context.TestEntities.CountAsync());
    }

    [Fact]
    public async Task EnablesChangeTracking_ForLinqQueries()
    {
        var context = TestDbContext.Create();
        context.TestEntities.Add(new TestEntity { Id = 1, Name = "Alice" });
        await context.SaveChangesAsync();
        context.ChangeTracker.Clear();
        using IWriteRepository<TestEntity> repo = new WriteRepository<TestEntity, TestDbContext>(context);

        await repo.ToListAsync();

        var entries = context.ChangeTracker.Entries<TestEntity>().ToList();
        Assert.Single(entries);
        Assert.Equal(EntityState.Unchanged, entries[0].State);
    }
}
