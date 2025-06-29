﻿using MockQueryable;
using Moq;

namespace Simpository.Mocking;
public static class Helpers
{
	public static Mock<TRepository> SetupEmptyData<TRepository, TEntity>(this Mock<TRepository> repo)
		where TEntity : class  // Ensures TEntity is a class
		where TRepository : class, IReadRepository<TEntity> // Ensures TRepo implements IReadRepository<TEntity>
	{
		return repo.SetupData(new List<TEntity>());
	}


	public static Mock<TRepository> SetupData<TRepository, TEntity>(this Mock<TRepository> repo, IEnumerable<TEntity> items)
		where TEntity : class  // Ensures TEntity is a class
		where TRepository : class, IReadRepository<TEntity> // Ensures TRepo implements IReadRepository<TEntity>
	{
		var queryable = items.BuildMock();

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
