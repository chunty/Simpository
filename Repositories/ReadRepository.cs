using System.Data;
using Microsoft.EntityFrameworkCore.Metadata;
using System.Collections;
using Simpository.Exceptions;

namespace Simpository.Repositories;

/// <summary>
/// A generic read-only repository implementation for entities of type <typeparamref name="T"/>.
/// Provides methods for retrieving and filtering entities from the database.
/// </summary>
/// <remarks>By default, to encourage read-only use of the repository, all returned entities are untracked.
/// For tracked results the preferred approach is to use a <see cref="WriteRepository{T,TContext}"/>.
/// Alternatively, call <see cref="SetQueryable"/> with <c>trackChanges = true</c>, or call <see cref="SetTrackingBehavior"/> with <c>true</c>.</remarks>
/// <typeparam name="T">The type of the entity managed by the repository.</typeparam>
/// <typeparam name="TContext"></typeparam>
public class ReadRepository<T, TContext> : IReadRepository<T> where T : class
where TContext : DbContext
{
	protected readonly TContext DbContext;
	private IQueryable<T> DbSet { get; set; } = null!;
	private bool _disposed = false; // Tracks the disposal state of the class.

	/// <summary>
	/// Initializes a new instance of the <see cref="ReadRepository{T}"/> class with the specified <paramref name="dbContext"/>.
	/// Sets the DbSet for the entity type <typeparamref name="T"/> to be queried.
	/// </summary>
	/// <param name="dbContext">The <see cref="Microsoft.EntityFrameworkCore.DbContext"/> instance used to access the database.</param>
	public ReadRepository(TContext dbContext)
	{
		DbContext = dbContext; // Store the provided DbContext
		SetQueryable(dbContext.Set<T>()); // Initialize the DbSet for the entity type
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="ReadRepository{T}"/> class with the specified <paramref name="dbContext"/> 
	/// and a custom <paramref name="queryable"/> to be used for querying.
	/// </summary>
	/// <param name="dbContext">The <see cref="Microsoft.EntityFrameworkCore.DbContext"/> instance used to access the database.</param>
	/// <param name="queryable">An <see cref="IQueryable{T}"/> that defines the set of entities to query.</param>
	protected ReadRepository(TContext dbContext, IQueryable<T> queryable) : this(dbContext)
	{
		SetQueryable(queryable); // Set the custom IQueryable for querying
	}

	/// <summary>
	/// Tries to find an entity of type <typeparamref name="T"/> by the specified key value.
	/// If the entity is not found, behavior is controlled by the <paramref name="errorOnNotFound"/> flag.
	/// </summary>
	/// <param name="keyValue">The key value to search for the entity.</param>
	/// <param name="errorOnNotFound">
	/// If set to <c>true</c>, an error is thrown if the entity is not found.
	/// If set to <c>false</c>, the method returns <c>null</c> when the entity is not found.
	/// </param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>
	/// A task representing the asynchronous operation. The task result contains the entity of type <typeparamref name="T"/> 
	/// if found, or <c>null</c> if not found and <paramref name="errorOnNotFound"/> is <c>false</c>.
	/// </returns>
	public async Task<T?> Find(object keyValue, CancellationToken cancellationToken = default) =>
		await Find([keyValue], cancellationToken);


	/// <summary>
	/// Tries to find an entity of type <typeparamref name="T"/> by the specified key values.
	/// If the entity is not found, behavior is controlled by the <paramref name="errorOnNotFound"/> flag.
	/// </summary>
	/// <param name="keyValues">An array of key values to search for the entity.</param>
	/// <param name="errorOnNotFound">
	/// If set to <c>true</c>, throws a <see cref="DataNotFoundException{T}"/> if the entity is not found.
	/// If set to <c>false</c>, the method returns <c>null</c> when the entity is not found.
	/// </param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>
	/// A task representing the asynchronous operation. The task result contains the entity of type <typeparamref name="T"/> 
	/// if found, or <c>null</c> if not found and <paramref name="errorOnNotFound"/> is <c>false</c>.
	/// </returns>
	/// <exception cref="DataNotFoundException{T}">Thrown if the entity is not found and <paramref name="errorOnNotFound"/> is <c>true</c>.</exception>
	public async Task<T?> Find(object[] keyValues, CancellationToken cancellationToken = default) =>
		await DbContext.FindAsync<T>(keyValues, cancellationToken);

	/// <summary>
	/// Asynchronously retrieves an entity of type <typeparamref name="T"/> by the specified key value.
	/// This method will throw an exception if the entity is not found.
	/// </summary>
	/// <param name="keyValue">The key value used to find the entity.</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>A task representing the asynchronous operation. The task result contains the entity of type <typeparamref name="T"/> if found.</returns>
	/// <exception cref="DataNotFoundException{T}">Thrown if the entity is not found for the given <paramref name="keyValue"/>.</exception>
	public async Task<T> FindOrThrow(object keyValue, CancellationToken cancellationToken = default)
	{
		var result = await Find(keyValue, cancellationToken) ??
					 throw new DataNotFoundException<T>(keyValue);

		return result;
	}

	/// <summary>
	/// Asynchronously retrieves an entity of type <typeparamref name="T"/> by the specified array of key values.
	/// This method will throw an exception if the entity is not found.
	/// </summary>
	/// <param name="keyValues">An array of key values used to find the entity.</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>A task representing the asynchronous operation. The task result contains the entity of type <typeparamref name="T"/> if found.</returns>
	/// <exception cref="DataNotFoundException{T}">Thrown if the entity is not found for the given <paramref name="keyValues"/>.</exception>
	public async Task<T> FindOrThrow(object[] keyValues, CancellationToken cancellationToken = default)
	{
		var result = await Find(keyValues, cancellationToken) ??
					 throw new DataNotFoundException<T>(keyValues);

		return result;

	}

	public async Task<T?> Get(object key, CancellationToken cancellationToken = default)
	{
		var primaryKey = GetPrimaryKey(); // Retrieve the primary key property of the entity

		// Build a lambda expression to match the entity by the key value
		var parameter = Expression.Parameter(typeof(T), "e");
		var keyProperty = Expression.Property(parameter, primaryKey.Name);
		var equality = Expression.Equal(keyProperty, Expression.Constant(key));
		var predicate = Expression.Lambda<Func<T, bool>>(equality, parameter);

		// Use the generated lambda in the query to find the entity
		return await DbSet.Where(predicate).SingleOrDefaultAsync(cancellationToken);
	}

	/// <summary>
	/// Retrieves an entity of type <typeparamref name="T"/> by the specified key.
	/// Throws a <see cref="DataNotFoundException{T}"/> if the entity is not found.
	/// </summary>
	/// <param name="key">The key value used to find the entity.</param>
	/// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
	/// <returns>
	/// A task representing the asynchronous operation. The task result contains the entity of type <typeparamref name="T"/> if found.
	/// </returns>
	/// <exception cref="DataNotFoundException{T}">
	/// Thrown if the entity is not found for the given <paramref name="key"/>.
	/// </exception>
	public async Task<T> GetOrThrow(object key, CancellationToken cancellationToken = default)
	{
		var result = await Get(key, cancellationToken);

		if (result is null)
		{
			var primaryKey = GetPrimaryKey(); // Retrieve the primary key property of the entity
			throw new DataNotFoundException<T>(key, primaryKey.Name); // Throw if not found
		}

		return result;
	}

	/// <summary>
	/// Sets the <see cref="IQueryable{T}"/> to be used for querying the entity type <typeparamref name="T"/>.
	/// Optionally configures whether to track changes to the entities retrieved from the queryable.
	/// </summary>
	/// <param name="queryable">The <see cref="IQueryable{T}"/> to set for querying.</param>
	/// <param name="trackChanges">
	/// A boolean value indicating whether to track changes to the entities.
	/// If <c>true</c>, changes to the entities will be tracked; if <c>false</c>, entities will be retrieved without tracking changes.
	/// </param>
	protected void SetQueryable(IQueryable<T> queryable, bool trackChanges = false)
	{
		DbSet = queryable; // Set the provided IQueryable for querying

		SetTrackingBehavior(trackChanges);
	}

	/// <summary>
	/// Configures the tracking behavior for entities retrieved from the <see cref="IQueryable{T}"/>.
	/// Enables or disables change tracking for the entities in the repository.
	/// </summary>
	/// <param name="trackingEnabled">
	/// A boolean value indicating whether to enable change tracking.
	/// If <c>true</c>, change tracking is enabled; if <c>false</c>, change tracking is disabled.
	/// </param>
	protected void SetTrackingBehavior(bool trackingEnabled)
	{
		DbSet = trackingEnabled ? DbSet.AsTracking() : // Enable change tracking for the DbSet
			DbSet.AsNoTracking(); // Disable change tracking for the DbSet
	}


	/// <summary>
	/// Retrieves the value of the primary key for the specified entity.
	/// Throws a <see cref="MissingPrimaryKeyException"/> if the primary key value is missing or <c>null</c>.
	/// </summary>
	/// <param name="entity">The entity of type <typeparamref name="T"/> from which to retrieve the primary key value.</param>
	/// <returns>The value of the primary key for the given entity.</returns>
	/// <exception cref="MissingPrimaryKeyException">
	/// Thrown if the primary key property is <c>null</c> or has no value.
	/// </exception>
	protected object GetPrimaryKeyValue(T entity)
	{
		var keyProperty = GetPrimaryKey(); // Retrieve the primary key property

		// Get the primary key value from the entity's entry in the DbContext
		return DbContext.Entry(entity)
				   .Property(keyProperty.Name)
				   .CurrentValue ??
			   throw new MissingPrimaryKeyException($"{keyProperty.Name} has no value");
	}

	/// <summary>
	/// Retrieves the primary key property metadata for the entity type <typeparamref name="T"/>.
	/// Throws a <see cref="MissingPrimaryKeyException"/> if the entity type or primary key cannot be found.
	/// </summary>
	/// <returns>The primary key property metadata for the entity type <typeparamref name="T"/>.</returns>
	/// <exception cref="MissingPrimaryKeyException">
	/// Thrown if the entity type cannot be found in the model or if the entity type has no primary key.
	/// </exception>
	private IProperty GetPrimaryKey()
	{
		// Get entity type metadata from the DbContext model
		var entityType = DbContext.Model.FindEntityType(typeof(T)) ??
						 throw new MissingPrimaryKeyException($"Cannot find entity type for {nameof(T)}");

		// Get the primary key metadata for the entity type
		var primaryKey = entityType.FindPrimaryKey() ??
						 throw new MissingPrimaryKeyException($"Type {nameof(T)} has no primary key");

		// Access the first primary key property (assuming the entity has a single primary key)
		var keyProperty = primaryKey.Properties[0];
		return keyProperty;
	}

	/// <summary>
	/// Releases resources owned by the object.
	/// </summary>
	/// <param name="disposing">True if disposing, false if finalizing.</param>

	protected virtual void Dispose(bool disposing)
	{
		if (!_disposed)
		{
			if (disposing)
			{
				DbContext.Dispose();
			}
		}
		_disposed = true;
	}



	/// <summary>
	/// Public implementation of Dispose pattern.
	/// </summary>
	public void Dispose()
	{
		Dispose(true);
		GC.SuppressFinalize(this);
	}

	public IEnumerator<T> GetEnumerator()
	{
		// ReSharper disable once NotDisposedResourceIsReturned
		return DbSet.GetEnumerator();
	}

	public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
		=> DbSet.AsAsyncEnumerable().GetAsyncEnumerator(cancellationToken);

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Type ElementType => DbSet.ElementType;
	public Expression Expression => DbSet.Expression;
	public IQueryProvider Provider => DbSet.Provider;
	
}