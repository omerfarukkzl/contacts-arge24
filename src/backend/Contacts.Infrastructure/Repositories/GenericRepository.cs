using Contacts.Api.Data;
using Contacts.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Repositories;

public class GenericRepository<T>(ContactsDbContext dbContext) : IGenericRepository<T>
    where T : class
{
    protected readonly ContactsDbContext DbContext = dbContext;

    public IQueryable<T> Query()
    {
        return DbContext.Set<T>();
    }

    public async Task AddAsync(T entity, CancellationToken cancellationToken)
    {
        await DbContext.Set<T>().AddAsync(entity, cancellationToken);
    }

    public async Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken)
    {
        await DbContext.Set<T>().AddRangeAsync(entities, cancellationToken);
    }

    public void Update(T entity)
    {
        DbContext.Set<T>().Update(entity);
    }

    public void Remove(T entity)
    {
        DbContext.Set<T>().Remove(entity);
    }
}
