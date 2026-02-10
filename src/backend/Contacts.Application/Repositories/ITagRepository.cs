using Contacts.Api.Domain.Entities;

namespace Contacts.Api.Repositories;

public interface ITagRepository : IGenericRepository<Tag>
{
    Task<List<Tag>> GetByOwnerAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<Tag?> GetByOwnerAndIdAsync(Guid ownerUserId, int tagId, CancellationToken cancellationToken);
    Task<bool> ExistsByOwnerAndNameAsync(Guid ownerUserId, string normalizedName, CancellationToken cancellationToken);
    Task<List<Tag>> GetByOwnerAndNamesAsync(Guid ownerUserId, IReadOnlyCollection<string> tagNames, CancellationToken cancellationToken);
}
