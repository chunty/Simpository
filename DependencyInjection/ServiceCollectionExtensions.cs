using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Simpository.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/> to register generic repository services
/// based on the <see cref="DbSet{T}"/> properties of a given <see cref="DbContext"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
	/// <summary>
	/// Registers a scoped <see cref="IReadRepository{T}"/> for every <see cref="DbSet{T}"/> property
	/// declared on <typeparamref name="TContext"/>.
	/// </summary>
	/// <typeparam name="TContext">The <see cref="DbContext"/> type to inspect for entity sets.</typeparam>
	/// <param name="services">The service collection to add registrations to.</param>
	/// <returns>The service collection, to allow method chaining.</returns>
	public static IServiceCollection AddGenericReadRepo<TContext>(this IServiceCollection services)
		=> services.RegisterRepositories<TContext>(typeof(IReadRepository<>), typeof(ReadRepository<,>));

	/// <summary>
	/// Registers a scoped <see cref="IWriteRepository{T}"/> for every <see cref="DbSet{T}"/> property
	/// declared on <typeparamref name="TContext"/>.
	/// </summary>
	/// <typeparam name="TContext">The <see cref="DbContext"/> type to inspect for entity sets.</typeparam>
	/// <param name="services">The service collection to add registrations to.</param>
	/// <returns>The service collection, to allow method chaining.</returns>
	public static IServiceCollection AddGenericWriteRepo<TContext>(this IServiceCollection services)
		=> services.RegisterRepositories<TContext>(typeof(IWriteRepository<>), typeof(WriteRepository<,>));

	/// <summary>
	/// Registers both <see cref="IReadRepository{T}"/> and <see cref="IWriteRepository{T}"/> for every
	/// <see cref="DbSet{T}"/> property declared on <typeparamref name="TContext"/>.
	/// Equivalent to calling <see cref="AddGenericReadRepo{TContext}"/> and
	/// <see cref="AddGenericWriteRepo{TContext}"/> in sequence.
	/// </summary>
	/// <typeparam name="TContext">The <see cref="DbContext"/> type to inspect for entity sets.</typeparam>
	/// <param name="services">The service collection to add registrations to.</param>
	/// <returns>The service collection, to allow method chaining.</returns>
	public static IServiceCollection AddGenericRepos<TContext>(this IServiceCollection services)
	{
		services.AddGenericReadRepo<TContext>();
		services.AddGenericWriteRepo<TContext>();
		return services;
	}

	private static IServiceCollection RegisterRepositories<TContext>(
		this IServiceCollection services,
		Type interfaceType,
		Type implementationType)
	{
		var contextType = typeof(TContext);
		var entityTypes = contextType
			.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
			.Where(p => p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>))
			.Select(p => p.PropertyType.GenericTypeArguments[0]);

		foreach (var entityType in entityTypes)
		{
			services.AddScoped(
				interfaceType.MakeGenericType(entityType),
				implementationType.MakeGenericType(entityType, contextType));
		}

		return services;
	}
}
