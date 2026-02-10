using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Repositories;

public interface IContactRepository : IGenericRepository<Contact>
{
    Task<int> CountByOwnerAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<List<Contact>> GetPagedByOwnerAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<Contact?> GetByOwnerAndIdAsync(Guid ownerUserId, Guid contactId, bool includeDeleted, CancellationToken cancellationToken);
    Task<bool> ExistsByPhoneAsync(Guid ownerUserId, string normalizedPhone, Guid? excludedContactId, CancellationToken cancellationToken);
    Task<List<Contact>> GetForExportAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
}
