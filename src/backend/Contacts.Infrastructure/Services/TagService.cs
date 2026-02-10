using Contacts.Api.Domain.Entities;
using Contacts.Api.Repositories;
using Microsoft.Extensions.Logging;

namespace Contacts.Api.Services;

public sealed class TagService(
    ITagRepository tagRepository,
    IUnitOfWork unitOfWork,
    ILogger<TagService> logger) : ITagService
{
    public async Task SyncContactTagsAsync(
        Contact contact,
        Guid ownerUserId,
        IEnumerable<string>? rawTagNames,
        CancellationToken cancellationToken)
    {
        var normalizedTagNames = NormalizeTagNames(rawTagNames);
        var tags = await ResolveTagsAsync(ownerUserId, normalizedTagNames, cancellationToken);
        var requestedTagIds = tags.Select(tag => tag.Id).ToHashSet();

        var relationsToDelete = contact.ContactTags
            .Where(contactTag => !requestedTagIds.Contains(contactTag.TagId))
            .ToList();

        foreach (var relation in relationsToDelete)
        {
            contact.ContactTags.Remove(relation);
        }

        var existingTagIds = contact.ContactTags
            .Select(contactTag => contactTag.TagId)
            .ToHashSet();

        foreach (var tag in tags.Where(tag => !existingTagIds.Contains(tag.Id)))
        {
            contact.ContactTags.Add(new ContactTag
            {
                ContactId = contact.Id,
                TagId = tag.Id,
                Tag = tag
            });
        }
    }

    private async Task<List<Tag>> ResolveTagsAsync(
        Guid ownerUserId,
        IReadOnlyCollection<string> normalizedTagNames,
        CancellationToken cancellationToken)
    {
        if (normalizedTagNames.Count == 0)
        {
            return [];
        }

        var existingTags = await tagRepository.GetByOwnerAndNamesAsync(ownerUserId, normalizedTagNames, cancellationToken);

        var existingNames = existingTags
            .Select(tag => tag.Name)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var newTags = normalizedTagNames
            .Where(tagName => !existingNames.Contains(tagName))
            .Select(tagName => new Tag
            {
                Name = tagName,
                OwnerUserId = ownerUserId
            })
            .ToList();

        if (newTags.Count > 0)
        {
            await tagRepository.AddRangeAsync(newTags, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            existingTags.AddRange(newTags);
            logger.LogInformation("Created {TagCount} new tags for owner {OwnerUserId}", newTags.Count, ownerUserId);
        }

        return existingTags;
    }

    private static List<string> NormalizeTagNames(IEnumerable<string>? rawTagNames)
    {
        if (rawTagNames is null)
        {
            return [];
        }

        return rawTagNames
            .Select(tagName => tagName.Trim())
            .Where(tagName => !string.IsNullOrWhiteSpace(tagName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }
}
