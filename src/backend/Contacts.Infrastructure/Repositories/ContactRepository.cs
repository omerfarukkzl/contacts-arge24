using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Contacts;
using Contacts.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Repositories;

public sealed class ContactRepository(ContactsDbContext dbContext) : GenericRepository<Contact>(dbContext), IContactRepository
{
    public Task<int> CountByOwnerAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken)
    {
        query.Normalize();

        return Query()
            .AsNoTracking()
            .Where(contact => contact.OwnerUserId == ownerUserId)
            .ApplyFilters(query)
            .CountAsync(cancellationToken);
    }

    public Task<List<Contact>> GetPagedByOwnerAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken)
    {
        query.Normalize();

        return Query()
            .AsNoTracking()
            .Where(contact => contact.OwnerUserId == ownerUserId)
            .Include(contact => contact.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .ApplyFilters(query)
            .ApplySorting(query)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);
    }

    public Task<Contact?> GetByOwnerAndIdAsync(
        Guid ownerUserId,
        Guid contactId,
        bool includeDeleted,
        CancellationToken cancellationToken)
    {
        var contactQuery = includeDeleted
            ? Query().IgnoreQueryFilters()
            : Query();

        return contactQuery
            .Where(contact => contact.OwnerUserId == ownerUserId)
            .Include(contact => contact.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .FirstOrDefaultAsync(contact => contact.Id == contactId, cancellationToken);
    }

    public Task<bool> ExistsByPhoneAsync(
        Guid ownerUserId,
        string normalizedPhone,
        Guid? excludedContactId,
        CancellationToken cancellationToken)
    {
        var query = Query().Where(contact => contact.OwnerUserId == ownerUserId && contact.Phone == normalizedPhone);

        if (excludedContactId.HasValue)
        {
            query = query.Where(contact => contact.Id != excludedContactId.Value);
        }

        return query.AnyAsync(cancellationToken);
    }

    public Task<List<Contact>> GetForExportAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken)
    {
        query.Normalize();

        return Query()
            .AsNoTracking()
            .Where(contact => contact.OwnerUserId == ownerUserId)
            .Include(contact => contact.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .ApplyFilters(query)
            .ApplySorting(query)
            .ToListAsync(cancellationToken);
    }
}
