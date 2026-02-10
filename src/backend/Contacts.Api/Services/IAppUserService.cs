using Contacts.Api.Domain.Entities;

namespace Contacts.Api.Services;

public interface IAppUserService
{
    Task<AppUser> GetOrCreateByFirebaseUidAsync(
        string firebaseUid,
        string? email,
        string? displayName,
        CancellationToken cancellationToken);
}
