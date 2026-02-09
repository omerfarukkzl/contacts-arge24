using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Services;

public static class ContactQueryExtensions
{
    public static IQueryable<Contact> ApplyFilters(this IQueryable<Contact> source, ContactListQuery query)
    {
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var searchTerm = query.Search.Trim();
            source = source.Where(contact =>
                contact.FirstName.Contains(searchTerm) ||
                contact.LastName.Contains(searchTerm) ||
                contact.Phone.Contains(searchTerm) ||
                (contact.Email != null && contact.Email.Contains(searchTerm)) ||
                (contact.Company != null && contact.Company.Contains(searchTerm)));
        }

        if (query.FavoriteOnly)
        {
            source = source.Where(contact => contact.IsFavorite);
        }

        if (!string.IsNullOrWhiteSpace(query.Tag))
        {
            var tagValue = query.Tag.Trim();
            source = source.Where(contact =>
                contact.ContactTags.Any(contactTag => contactTag.Tag.Name == tagValue));
        }

        if (!string.IsNullOrWhiteSpace(query.Company))
        {
            var companyValue = query.Company.Trim();
            source = source.Where(contact =>
                contact.Company != null && contact.Company.Contains(companyValue));
        }

        return source;
    }

    public static IQueryable<Contact> ApplySorting(this IQueryable<Contact> source, ContactListQuery query)
    {
        var sortBy = query.SortBy.Trim().ToLowerInvariant();

        return (sortBy, query.SortDir) switch
        {
            ("lastname", "desc") => source.OrderByDescending(contact => contact.LastName).ThenBy(contact => contact.FirstName),
            ("lastname", _) => source.OrderBy(contact => contact.LastName).ThenBy(contact => contact.FirstName),
            ("createdat", "desc") => source.OrderByDescending(contact => contact.CreatedAt),
            ("createdat", _) => source.OrderBy(contact => contact.CreatedAt),
            (_, "desc") => source.OrderByDescending(contact => contact.FirstName).ThenByDescending(contact => contact.LastName),
            _ => source.OrderBy(contact => contact.FirstName).ThenBy(contact => contact.LastName)
        };
    }
}
