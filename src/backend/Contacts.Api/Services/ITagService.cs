using Contacts.Api.Domain.Entities;

namespace Contacts.Api.Services;

public interface ITagService
{
    Task SyncContactTagsAsync(
        Contact contact,
        Guid ownerUserId,
        IEnumerable<string>? rawTagNames,
        CancellationToken cancellationToken);
}
