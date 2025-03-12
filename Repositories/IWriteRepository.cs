namespace Simpository.Repositories;
public interface IWriteRepository<T> : IReadRepository<T> where T : class
{
	Task<T> Add(T entity, CancellationToken cancellationToken = default);
	Task<IEnumerable<T>> Add(IEnumerable<T> entity, CancellationToken cancellationToken = default);
	Task<T> Update(T entity, CancellationToken cancellationToken = default);
	Task<IEnumerable<T>> Update(IEnumerable<T> entities, CancellationToken cancellationToken = default);
	Task Delete(T entity, CancellationToken cancellationToken = default);
	Task Delete(object key, CancellationToken cancellationToken = default);
	Task Delete(IEnumerable<T> entity, CancellationToken cancellationToken = default);
}