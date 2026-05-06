using Microsoft.Extensions.DependencyInjection;
using Simpository.DependencyInjection;

namespace Simpository.Tests.DependencyInjection;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGenericReadRepo_RegistersReadRepositoryForEachDbSet()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(opt => opt.UseInMemoryDatabase("read-resolve"));
        services.AddGenericReadRepo<TestDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repo = scope.ServiceProvider.GetService<IReadRepository<TestEntity>>();

        Assert.NotNull(repo);
        Assert.IsType<ReadRepository<TestEntity, TestDbContext>>(repo);
    }

    [Fact]
    public void AddGenericWriteRepo_RegistersWriteRepositoryForEachDbSet()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(opt => opt.UseInMemoryDatabase("write-resolve"));
        services.AddGenericWriteRepo<TestDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var repo = scope.ServiceProvider.GetService<IWriteRepository<TestEntity>>();

        Assert.NotNull(repo);
        Assert.IsType<WriteRepository<TestEntity, TestDbContext>>(repo);
    }

    [Fact]
    public void AddGenericRepos_RegistersBothReadAndWriteRepositories()
    {
        var services = new ServiceCollection();
        services.AddDbContext<TestDbContext>(opt => opt.UseInMemoryDatabase("both-resolve"));
        services.AddGenericRepos<TestDbContext>();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var readRepo = scope.ServiceProvider.GetService<IReadRepository<TestEntity>>();
        var writeRepo = scope.ServiceProvider.GetService<IWriteRepository<TestEntity>>();

        Assert.NotNull(readRepo);
        Assert.NotNull(writeRepo);
    }

    [Fact]
    public void AddGenericReadRepo_RegistersAsScoped()
    {
        var services = new ServiceCollection();
        services.AddGenericReadRepo<TestDbContext>();

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IReadRepository<TestEntity>));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddGenericWriteRepo_RegistersAsScoped()
    {
        var services = new ServiceCollection();
        services.AddGenericWriteRepo<TestDbContext>();

        var descriptor = services.FirstOrDefault(
            d => d.ServiceType == typeof(IWriteRepository<TestEntity>));

        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }
}
