using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Repositories;

public sealed class TagRepository(ContactsDbContext dbContext) : GenericRepository<Tag>(dbContext), ITagRepository
{
    public Task<List<Tag>> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        return Query()
            .AsNoTracking()
            .Where(tag => tag.OwnerUserId == ownerUserId)
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);
    }

    public Task<Tag?> GetByOwnerAndIdAsync(Guid ownerUserId, int tagId, CancellationToken cancellationToken)
    {
        return Query()
            .Where(tag => tag.OwnerUserId == ownerUserId)
            .FirstOrDefaultAsync(tag => tag.Id == tagId, cancellationToken);
    }

    public Task<bool> ExistsByOwnerAndNameAsync(Guid ownerUserId, string normalizedName, CancellationToken cancellationToken)
    {
        return Query()
            .AnyAsync(
                tag => tag.OwnerUserId == ownerUserId && tag.Name.ToLower() == normalizedName.ToLower(),
                cancellationToken);
    }

    public Task<List<Tag>> GetByOwnerAndNamesAsync(Guid ownerUserId, IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken)
    {
        if (tagNames.Count == 0)
        {
            return Task.FromResult(new List<Tag>());
        }

        var normalizedLower = tagNames
            .Select(tagName => tagName.ToLowerInvariant())
            .ToList();

        return Query()
            .Where(tag => tag.OwnerUserId == ownerUserId && normalizedLower.Contains(tag.Name.ToLower()))
            .ToListAsync(cancellationToken);
    }
}
