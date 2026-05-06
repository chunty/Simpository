namespace Simpository.Repositories;

/// <summary>
/// Defines a read-write repository for entities of type <typeparamref name="T"/>.
/// Extends <see cref="IReadRepository{T}"/> with add, update, and delete operations.
/// </summary>
/// <remarks>Change tracking is enabled by default to support mutation operations.</remarks>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public interface IWriteRepository<T> : IReadRepository<T> where T : class
{
	/// <summary>
	/// Asynchronously adds a new entity to the data store.
	/// </summary>
	/// <param name="entity">The entity to add.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The added entity.</returns>
	Task<T> Add(T entity, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously adds a collection of entities to the data store.
	/// Returns an empty collection immediately if <paramref name="entity"/> is empty.
	/// </summary>
	/// <param name="entity">The entities to add.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The added entities, or an empty collection if none were provided.</returns>
	Task<IEnumerable<T>> Add(IEnumerable<T> entity, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously updates an existing entity in the data store.
	/// </summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The updated entity.</returns>
	Task<T> Update(T entity, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously updates a collection of existing entities in the data store.
	/// Returns an empty collection immediately if <paramref name="entities"/> is empty.
	/// </summary>
	/// <param name="entities">The entities to update.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <returns>The updated entities, or an empty collection if none were provided.</returns>
	Task<IEnumerable<T>> Update(IEnumerable<T> entities, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously deletes an entity from the data store.
	/// </summary>
	/// <param name="entity">The entity to delete.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	Task Delete(T entity, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously deletes an entity from the data store by its primary key.
	/// </summary>
	/// <param name="key">The primary key of the entity to delete.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	/// <exception cref="Exceptions.DataNotFoundException{T}">Thrown when no entity with the given key exists.</exception>
	Task Delete(object key, CancellationToken cancellationToken = default);

	/// <summary>
	/// Asynchronously deletes a collection of entities from the data store.
	/// </summary>
	/// <param name="entity">The entities to delete.</param>
	/// <param name="cancellationToken">A token to cancel the operation.</param>
	Task Delete(IEnumerable<T> entity, CancellationToken cancellationToken = default);
}