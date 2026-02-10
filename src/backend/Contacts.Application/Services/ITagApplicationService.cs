using Contacts.Api.Dtos.Tags;

namespace Contacts.Api.Services;

public interface ITagApplicationService
{
    Task<IReadOnlyList<TagDto>> GetTagsAsync(Guid ownerUserId, CancellationToken cancellationToken);
    Task<TagCreateResult> CreateTagAsync(Guid ownerUserId, CreateTagRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteTagAsync(Guid ownerUserId, int tagId, CancellationToken cancellationToken);
}

public sealed record TagCreateResult(
    bool IsDuplicate,
    TagDto? Tag);
