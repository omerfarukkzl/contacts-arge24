namespace Contacts.Api.Repositories;

public interface IGenericRepository<T>
    where T : class
{
    IQueryable<T> Query();
    Task AddAsync(T entity, CancellationToken cancellationToken);
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken);
    void Update(T entity);
    void Remove(T entity);
}
