using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Simpository.DependencyInjection;
public static class ServiceCollectionExtensions
{
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

	public static IServiceCollection AddGenericRepos<TContext>(this IServiceCollection registry)
	{
		registry.AddGenericReadRepo<TContext>();
		registry.AddGenericWriteRepo<TContext>();

		return registry;
	}
}
