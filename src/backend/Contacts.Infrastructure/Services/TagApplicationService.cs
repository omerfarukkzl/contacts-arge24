using AutoMapper;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Tags;
using Contacts.Api.Repositories;
using Microsoft.Extensions.Logging;

namespace Contacts.Api.Services;

public sealed class TagApplicationService(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<TagApplicationService> logger) : ITagApplicationService
{
    public async Task<IReadOnlyList<TagDto>> GetTagsAsync(Guid ownerUserId, CancellationToken cancellationToken)
    {
        var tags = await tagRepository.GetByOwnerAsync(ownerUserId, cancellationToken);
        return mapper.Map<List<TagDto>>(tags);
    }

    public async Task<TagCreateResult> CreateTagAsync(
        Guid ownerUserId,
        CreateTagRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var duplicateExists = await tagRepository.ExistsByOwnerAndNameAsync(ownerUserId, normalizedName, cancellationToken);
        if (duplicateExists)
        {
            return new TagCreateResult(true, null);
        }

        var tag = new Tag
        {
            OwnerUserId = ownerUserId,
            Name = normalizedName
        };

        await tagRepository.AddAsync(tag, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created tag {TagId} for owner {OwnerUserId}", tag.Id, ownerUserId);
        return new TagCreateResult(false, mapper.Map<TagDto>(tag));
    }

    public async Task<bool> DeleteTagAsync(Guid ownerUserId, int tagId, CancellationToken cancellationToken)
    {
        var tag = await tagRepository.GetByOwnerAndIdAsync(ownerUserId, tagId, cancellationToken);
        if (tag is null)
        {
            return false;
        }

        tagRepository.Remove(tag);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Deleted tag {TagId} for owner {OwnerUserId}", tagId, ownerUserId);
        return true;
    }
}
