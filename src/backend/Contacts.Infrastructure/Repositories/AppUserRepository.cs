using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Repositories;

public sealed class AppUserRepository(ContactsDbContext dbContext) : GenericRepository<AppUser>(dbContext), IAppUserRepository
{
    public Task<AppUser?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken)
    {
        return Query()
            .FirstOrDefaultAsync(user => user.FirebaseUid == firebaseUid, cancellationToken);
    }
}
