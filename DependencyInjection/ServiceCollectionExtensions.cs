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
	{
		var t = typeof(TContext);
		var properties = t.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
			.Where(i => i.PropertyType.IsGenericType && i.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
		var types = properties.Select(p => p.PropertyType.GenericTypeArguments[0]).ToArray();

		foreach (var type in types)
		{
			services.AddScoped(typeof(IReadRepository<>).MakeGenericType(type), typeof(ReadRepository<,>).MakeGenericType(type, t));
		}

		return services;
	}

	/// <summary>
	/// Registers a scoped <see cref="IWriteRepository{T}"/> for every <see cref="DbSet{T}"/> property
	/// declared on <typeparamref name="TContext"/>.
	/// </summary>
	/// <typeparam name="TContext">The <see cref="DbContext"/> type to inspect for entity sets.</typeparam>
	/// <param name="registry">The service collection to add registrations to.</param>
	/// <returns>The service collection, to allow method chaining.</returns>
	public static IServiceCollection AddGenericWriteRepo<TContext>(this IServiceCollection registry)
	{
		var t = typeof(TContext);
		var properties = t.GetProperties(BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public)
			.Where(i => i.PropertyType.IsGenericType && i.PropertyType.GetGenericTypeDefinition() == typeof(DbSet<>));
		var types = properties.Select(p => p.PropertyType.GenericTypeArguments[0]).ToArray();

		foreach (var type in types)
		{
			registry.AddScoped(typeof(IWriteRepository<>).MakeGenericType(type), typeof(WriteRepository<,>).MakeGenericType(type, t));
		}

		return registry;
	}

	/// <summary>
	/// Registers both <see cref="IReadRepository{T}"/> and <see cref="IWriteRepository{T}"/> for every
	/// <see cref="DbSet{T}"/> property declared on <typeparamref name="TContext"/>.
	/// Equivalent to calling <see cref="AddGenericReadRepo{TContext}"/> and
	/// <see cref="AddGenericWriteRepo{TContext}"/> in sequence.
	/// </summary>
	/// <typeparam name="TContext">The <see cref="DbContext"/> type to inspect for entity sets.</typeparam>
	/// <param name="registry">The service collection to add registrations to.</param>
	/// <returns>The service collection, to allow method chaining.</returns>
	public static IServiceCollection AddGenericRepos<TContext>(this IServiceCollection registry)
	{
		registry.AddGenericReadRepo<TContext>();
		registry.AddGenericWriteRepo<TContext>();

		return registry;
	}
}
