using Contacts.Api.Domain.Entities;

namespace Contacts.Api.Repositories;

public interface IAppUserRepository : IGenericRepository<AppUser>
{
    Task<AppUser?> GetByFirebaseUidAsync(string firebaseUid, CancellationToken cancellationToken);
}
