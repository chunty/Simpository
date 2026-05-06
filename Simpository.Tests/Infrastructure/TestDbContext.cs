namespace Simpository.Tests.Infrastructure;

public class TestDbContext : DbContext
{
    public DbSet<TestEntity> TestEntities { get; set; } = null!;
    public DbSet<CompositeKeyEntity> CompositeKeyEntities { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<CompositeKeyEntity>()
            .HasKey(e => new { e.OrderId, e.LineNumber });
    }

    public static TestDbContext Create(string? name = null)
    {
        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .Options;
        return new TestDbContext(options);
    }
}
