using Contacts.Api.Data;

namespace Contacts.Api.Repositories;

public sealed class UnitOfWork(ContactsDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
    {
        return dbContext.SaveChangesAsync(cancellationToken);
    }
}
