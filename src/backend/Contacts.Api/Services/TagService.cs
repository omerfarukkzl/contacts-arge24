using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Services;

public sealed class TagService(ContactsDbContext dbContext) : ITagService
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

        var normalizedLower = normalizedTagNames
            .Select(tagName => tagName.ToLowerInvariant())
            .ToList();

        var existingTags = await dbContext.Tags
            .Where(tag => tag.OwnerUserId == ownerUserId && normalizedLower.Contains(tag.Name.ToLower()))
            .ToListAsync(cancellationToken);

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
            await dbContext.Tags.AddRangeAsync(newTags, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            existingTags.AddRange(newTags);
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
