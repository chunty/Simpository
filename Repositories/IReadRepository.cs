namespace Simpository.Repositories;
public interface IReadRepository<T> : IQueryable<T>, IDisposable where T : class
{
	Task<T?> Find(object keyValue, CancellationToken cancellationToken = default);
	Task<T?> Find(object[] keyValues, CancellationToken cancellationToken = default);
	Task<T> FindOrThrow(object keyValue, CancellationToken cancellationToken = default);
	Task<T> FindOrThrow(object[] keyValues, CancellationToken cancellationToken = default);
	Task<T?> Get(object key, CancellationToken cancellationToken = default);
	Task<T> GetOrThrow(object key, CancellationToken cancellationToken = default);
}
