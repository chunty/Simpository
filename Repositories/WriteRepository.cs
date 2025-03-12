namespace Simpository.Repositories;

public class WriteRepository<T, TContext> : ReadRepository<T, TContext>, IWriteRepository<T> where T : class
where TContext : DbContext
{
	/// <summary>
	/// Initializes a new instance of the <see cref="WriteRepository{T,TContext}"/> class with the specified <paramref name="dbContext"/>.
	/// Sets the tracking behavior to enable change tracking for entities.
	/// </summary>
	/// <param name="dbContext">The <see cref="DbContext"/> instance used to access the database.</param>
	public WriteRepository(TContext dbContext) : base(dbContext)
	{
		SetTrackingBehavior(true); // Enable change tracking for the entities
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="WriteRepository{T,TContext}"/> class with the specified <paramref name="dbContext"/> 
	/// and a custom <paramref name="queryable"/> to be used for querying. 
	/// Sets the tracking behavior to enable change tracking for the entities.
	/// </summary>
	/// <param name="dbContext">The <see cref="DbContext"/> instance used to access the database.</param>
	/// <param name="queryable">An <see cref="IQueryable{T}"/> that defines the set of entities to query.</param>
	protected WriteRepository(TContext dbContext, IQueryable<T> queryable) : base(dbContext, queryable)
	{
		SetQueryable(queryable, true); // Set the custom IQueryable and enable tracking
	}

	/// <summary>
	/// Asynchronously adds a new entity of type T to the database context.
	/// </summary>
	/// <param name="entity">The entity to add.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the add operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the added entity of type T.</returns>
	public virtual async Task<T> Add(T entity, CancellationToken cancellationToken = default)
	{
		await DbContext.AddAsync(entity, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);

		return entity;
	}

	/// <summary>
	/// Asynchronously adds multiple new entities of type T to the database context.
	/// </summary>
	/// <param name="entities">An enumerable of entities to add.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the add range operation. Defaults to default(CancellationToken).</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains an enumerable of added entities of type T.</returns>
	public virtual async Task<IEnumerable<T>> Add(IEnumerable<T> entities, CancellationToken cancellationToken = default)
	{
		var enumerable = entities.ToList();
		if (enumerable.Count == 0)
			return [];

		await DbContext.AddRangeAsync(enumerable, cancellationToken);
		await DbContext.SaveChangesAsync(cancellationToken);

		return enumerable;
	}

	/// <summary>
	/// Asynchronously updates an existing entity of type T in the database context.
	/// </summary>
	/// <param name="entity">The entity to update.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the update operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the updated entity of type T.</returns>
	public virtual async Task<T> Update(T entity, CancellationToken cancellationToken = default)
	{
		DbContext.Update(entity);
		await DbContext.SaveChangesAsync(cancellationToken);

		return entity;
	}

	/// <summary>
	/// Asynchronously updates multiple existing entities of type T in the database context.
	/// </summary>
	/// <param name="entities">An enumerable of entities to update.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the update range operation.</param>
	/// <returns>A task that represents the asynchronous operation. The task result contains the enumerable of updated entities of type T.</returns>
	public virtual async Task<IEnumerable<T>> Update(IEnumerable<T> entities, CancellationToken cancellationToken = default)
	{
		var enumerable = entities.ToList();
		if (enumerable.Count == 0)
			return [];

		DbContext.UpdateRange(enumerable);
		await DbContext.SaveChangesAsync(cancellationToken);

		return enumerable;
	}

	/// <summary>
	/// Asynchronously deletes an entity of type T from the database context.
	/// </summary>
	/// <param name="entity">The entity to delete.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation. Defaults to default(CancellationToken).</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public virtual async Task Delete(T entity, CancellationToken cancellationToken = default)
	{
		await Delete([entity], cancellationToken);
	}

	/// <summary>
	/// Asynchronously deletes an entity of type T from the database context by its identifier.
	/// </summary>
	/// <param name="key">The identifier of the entity to delete.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the delete operation. Defaults to default(CancellationToken).</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public virtual async Task Delete(object key, CancellationToken cancellationToken = default)
	{
		await Delete(await FindOrThrow(key, cancellationToken), cancellationToken: cancellationToken);
	}

	/// <summary>
	/// Asynchronously deletes multiple entities of type T from the database context.
	/// </summary>
	/// <param name="entities">An enumerable of entities to delete.</param>
	/// <param name="cancellationToken">A cancellation token that can be used to cancel the delete range operation. Defaults to default(CancellationToken).</param>
	/// <returns>A task that represents the asynchronous operation.</returns>
	public virtual async Task Delete(IEnumerable<T> entities, CancellationToken cancellationToken = default)
	{
		DbContext.RemoveRange(entities);
		await DbContext.SaveChangesAsync(cancellationToken);
	}
}

