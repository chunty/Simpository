namespace Simpository.Repositories;

/// <summary>
/// Defines a read-only repository for entities of type <typeparamref name="T"/>.
/// Supports querying via LINQ, asynchronous enumeration, and key-based lookups.
/// </summary>
/// <remarks>
/// By default, entities are returned without change tracking to encourage read-only usage.
/// Use <see cref="IWriteRepository{T}"/> when you need tracked entities for mutations.
/// </remarks>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public interface IReadRepository<T> : IQueryable<T>, IAsyncEnumerable<T>, IDisposable where T : class
{
	/// <summary>
	/// Asynchronously finds an entity by a single key value using the underlying <see cref="Microsoft.EntityFrameworkCore.DbContext"/>.
	/// </summary>
	/// <param name="keyValue">The key value to search for.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entity, or <c>null</c> if not found.</returns>
	Task<T?> Find(object keyValue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously finds an entity by an array of key values, supporting composite primary keys.
	/// </summary>
	/// <param name="keyValues">The key values to search for.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entity, or <c>null</c> if not found.</returns>
	Task<T?> Find(object[] keyValues, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously finds an entity by a single key value, throwing if not found.
	/// </summary>
	/// <param name="keyValue">The key value to search for.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entity.</returns>
	/// <exception cref="Exceptions.DataNotFoundException{T}">Thrown when no entity with the given key exists.</exception>
	Task<T> FindOrThrow(object keyValue, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously finds an entity by an array of key values, throwing if not found.
	/// </summary>
	/// <param name="keyValues">The key values to search for.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entity.</returns>
	/// <exception cref="Exceptions.DataNotFoundException{T}">Thrown when no entity with the given keys exists.</exception>
	Task<T> FindOrThrow(object[] keyValues, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously retrieves an entity by its primary key using a LINQ query.
	/// Unlike <see cref="Find(object, CancellationToken)"/>, this always queries the database directly.
	/// </summary>
	/// <param name="key">The primary key value.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entity, or <c>null</c> if not found.</returns>
	Task<T?> Get(object key, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously retrieves an entity by its primary key, throwing if not found.
	/// </summary>
	/// <param name="key">The primary key value.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The matching entity.</returns>
	/// <exception cref="Exceptions.DataNotFoundException{T}">Thrown when no entity with the given key exists.</exception>
	Task<T> GetOrThrow(object key, CancellationToken cancellationToken = default);
}
