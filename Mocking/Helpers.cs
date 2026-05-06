using MockQueryable;
using Moq;

namespace Simpository.Mocking;

/// <summary>
/// Provides extension methods for configuring <see cref="Moq.Mock{T}"/> instances of
/// <see cref="IReadRepository{T}"/> to return in-memory test data.
/// </summary>
public static class Helpers
{
	/// <summary>
	/// Configures the repository mock to return an empty data set, supporting both
	/// synchronous LINQ queries and asynchronous enumeration.
	/// </summary>
	/// <typeparam name="TRepository">The repository interface type being mocked.</typeparam>
	/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
	/// <param name="repo">The mock to configure.</param>
	/// <returns>The same mock instance, to allow method chaining.</returns>
	public static Mock<TRepository> SetupEmptyData<TRepository, TEntity>(this Mock<TRepository> repo)
		where TEntity : class
		where TRepository : class, IReadRepository<TEntity>
	{
		return repo.SetupData(new List<TEntity>());
	}

	/// <summary>
	/// Configures the repository mock to return the specified data set, supporting both
	/// synchronous LINQ queries and asynchronous enumeration via <c>ToListAsync</c> and
	/// <c>await foreach</c>.
	/// </summary>
	/// <typeparam name="TRepository">The repository interface type being mocked.</typeparam>
	/// <typeparam name="TEntity">The entity type managed by the repository.</typeparam>
	/// <param name="repo">The mock to configure.</param>
	/// <param name="items">The data to return from the mock.</param>
	/// <returns>The same mock instance, to allow method chaining.</returns>
	public static Mock<TRepository> SetupData<TRepository, TEntity>(this Mock<TRepository> repo, IEnumerable<TEntity> items)
		where TEntity : class
		where TRepository : class, IReadRepository<TEntity>
	{
		var queryable = items.ToList().BuildMock();

		repo.Setup(x => x.Expression).Returns(queryable.Expression);
		repo.Setup(x => x.ElementType).Returns(queryable.ElementType);
		repo.Setup(x => x.Provider).Returns(queryable.Provider);
		repo.Setup(x => x.GetEnumerator()).Returns(() => queryable.GetEnumerator());

		// This is needed to prevent an exception when calling repo.ToListAsync from tests.
		var asyncEnumerable = (IAsyncEnumerable<TEntity>)queryable;
		repo.Setup(x => x.GetAsyncEnumerator(It.IsAny<CancellationToken>()))
			.Returns(() => asyncEnumerable.GetAsyncEnumerator());

		return repo;
	}
}
