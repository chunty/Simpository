using Simpository.Mocking;

namespace Simpository.Tests.Mocking;

public class HelpersTests
{
    [Fact]
    public async Task SetupData_WithItems_ReturnsExpectedEntities()
    {
        var mockRepo = new Mock<ITestEntityRepository>();
        mockRepo.SetupData<ITestEntityRepository, TestEntity>(new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" }
        });

        var result = await mockRepo.Object.ToListAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task SetupEmptyData_ReturnsEmptyCollection()
    {
        var mockRepo = new Mock<ITestEntityRepository>();
        mockRepo.SetupEmptyData<ITestEntityRepository, TestEntity>();

        var result = await mockRepo.Object.ToListAsync();

        Assert.Empty(result);
    }

    [Fact]
    public void SetupData_SupportsLinqFiltering()
    {
        var mockRepo = new Mock<ITestEntityRepository>();
        mockRepo.SetupData<ITestEntityRepository, TestEntity>(new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" },
            new TestEntity { Id = 3, Name = "Charlie" }
        });

        var result = mockRepo.Object.Where(e => e.Name.StartsWith("A")).ToList();

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }

    [Fact]
    public async Task SetupData_SupportsAsyncEnumeration()
    {
        var mockRepo = new Mock<ITestEntityRepository>();
        mockRepo.SetupData<ITestEntityRepository, TestEntity>(new List<TestEntity>
        {
            new TestEntity { Id = 1, Name = "Alice" },
            new TestEntity { Id = 2, Name = "Bob" }
        });

        var results = new List<TestEntity>();
        await foreach (var entity in mockRepo.Object)
            results.Add(entity);

        Assert.Equal(2, results.Count);
    }

    [Fact]
    public void SetupData_ReturnsMockForChaining()
    {
        var mockRepo = new Mock<ITestEntityRepository>();

        var returned = mockRepo.SetupData<ITestEntityRepository, TestEntity>(new List<TestEntity>());

        Assert.Same(mockRepo, returned);
    }

    [Fact]
    public void SetupEmptyData_ReturnsMockForChaining()
    {
        var mockRepo = new Mock<ITestEntityRepository>();

        var returned = mockRepo.SetupEmptyData<ITestEntityRepository, TestEntity>();

        Assert.Same(mockRepo, returned);
    }
}
