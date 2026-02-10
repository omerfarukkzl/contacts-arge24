using AutoMapper;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Common;
using Contacts.Api.Dtos.Contacts;
using Contacts.Api.Exceptions;
using Contacts.Api.Repositories;
using Microsoft.Extensions.Logging;

namespace Contacts.Api.Services;

public sealed class ContactApplicationService(
    IContactRepository contactRepository,
    ITagService tagService,
    ICsvContactService csvContactService,
    IUnitOfWork unitOfWork,
    IMapper mapper,
    ILogger<ContactApplicationService> logger) : IContactApplicationService
{
    public async Task<PagedResult<ContactDto>> GetContactsAsync(
        Guid ownerUserId,
        ContactListQuery query,
        CancellationToken cancellationToken)
    {
        query.Normalize();

        var totalCount = await contactRepository.CountByOwnerAsync(ownerUserId, query, cancellationToken);
        var contacts = await contactRepository.GetPagedByOwnerAsync(ownerUserId, query, cancellationToken);
        var mapped = mapper.Map<List<ContactDto>>(contacts);

        return new PagedResult<ContactDto>(mapped, query.Page, query.PageSize, totalCount);
    }

    public async Task<ContactDto?> GetContactAsync(Guid ownerUserId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByOwnerAndIdAsync(ownerUserId, contactId, includeDeleted: false, cancellationToken);
        return contact is null ? null : mapper.Map<ContactDto>(contact);
    }

    public async Task<ContactDto> CreateContactAsync(
        Guid ownerUserId,
        CreateContactRequest request,
        CancellationToken cancellationToken)
    {
        var normalizedPhone = request.Phone.Trim();
        var duplicatePhoneExists = await contactRepository.ExistsByPhoneAsync(
            ownerUserId,
            normalizedPhone,
            excludedContactId: null,
            cancellationToken);

        if (duplicatePhoneExists)
        {
            throw new DuplicatePhoneException("A contact with the same phone number already exists.");
        }

        var now = DateTime.UtcNow;
        var contact = new Contact
        {
            OwnerUserId = ownerUserId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = normalizedPhone,
            Email = NullIfWhitespace(request.Email),
            Company = NullIfWhitespace(request.Company),
            Notes = NullIfWhitespace(request.Notes),
            IsFavorite = request.IsFavorite,
            CreatedAt = now,
            UpdatedAt = now
        };

        await tagService.SyncContactTagsAsync(contact, ownerUserId, request.Tags, cancellationToken);

        await contactRepository.AddAsync(contact, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created contact {ContactId} for owner {OwnerUserId}", contact.Id, ownerUserId);

        var createdContact = await contactRepository.GetByOwnerAndIdAsync(ownerUserId, contact.Id, includeDeleted: false, cancellationToken);
        return mapper.Map<ContactDto>(createdContact ?? contact);
    }

    public async Task<ContactDto?> UpdateContactAsync(
        Guid ownerUserId,
        Guid contactId,
        UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByOwnerAndIdAsync(ownerUserId, contactId, includeDeleted: false, cancellationToken);
        if (contact is null)
        {
            return null;
        }

        var normalizedPhone = request.Phone.Trim();
        var duplicatePhoneExists = await contactRepository.ExistsByPhoneAsync(
            ownerUserId,
            normalizedPhone,
            excludedContactId: contactId,
            cancellationToken);

        if (duplicatePhoneExists)
        {
            throw new DuplicatePhoneException("A contact with the same phone number already exists.");
        }

        contact.FirstName = request.FirstName.Trim();
        contact.LastName = request.LastName.Trim();
        contact.Phone = normalizedPhone;
        contact.Email = NullIfWhitespace(request.Email);
        contact.Company = NullIfWhitespace(request.Company);
        contact.Notes = NullIfWhitespace(request.Notes);
        contact.IsFavorite = request.IsFavorite;
        contact.UpdatedAt = DateTime.UtcNow;

        await tagService.SyncContactTagsAsync(contact, ownerUserId, request.Tags, cancellationToken);

        contactRepository.Update(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Updated contact {ContactId} for owner {OwnerUserId}", contact.Id, ownerUserId);

        return mapper.Map<ContactDto>(contact);
    }

    public async Task<bool> DeleteContactAsync(Guid ownerUserId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByOwnerAndIdAsync(ownerUserId, contactId, includeDeleted: false, cancellationToken);
        if (contact is null)
        {
            return false;
        }

        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;

        contactRepository.Update(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Soft-deleted contact {ContactId} for owner {OwnerUserId}", contactId, ownerUserId);
        return true;
    }

    public async Task<RestoreContactResult> RestoreContactAsync(Guid ownerUserId, Guid contactId, CancellationToken cancellationToken)
    {
        var contact = await contactRepository.GetByOwnerAndIdAsync(ownerUserId, contactId, includeDeleted: true, cancellationToken);

        if (contact is null)
        {
            return new RestoreContactResult(false, false, null);
        }

        if (!contact.IsDeleted)
        {
            return new RestoreContactResult(true, false, mapper.Map<ContactDto>(contact));
        }

        var duplicatePhoneExists = await contactRepository.ExistsByPhoneAsync(
            ownerUserId,
            contact.Phone,
            excludedContactId: contactId,
            cancellationToken);

        if (duplicatePhoneExists)
        {
            return new RestoreContactResult(true, true, mapper.Map<ContactDto>(contact));
        }

        contact.IsDeleted = false;
        contact.DeletedAt = null;
        contact.UpdatedAt = DateTime.UtcNow;

        contactRepository.Update(contact);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Restored contact {ContactId} for owner {OwnerUserId}", contactId, ownerUserId);
        return new RestoreContactResult(true, false, mapper.Map<ContactDto>(contact));
    }

    public Task<byte[]> ExportCsvAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken)
    {
        return csvContactService.ExportCsvAsync(ownerUserId, query, cancellationToken);
    }

    public Task<byte[]> ExportExcelAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken)
    {
        return csvContactService.ExportExcelAsync(ownerUserId, query, cancellationToken);
    }

    public Task<CsvImportResultDto> ImportCsvAsync(
        Guid ownerUserId,
        Stream stream,
        long fileLength,
        CancellationToken cancellationToken)
    {
        return csvContactService.ImportAsync(ownerUserId, stream, fileLength, cancellationToken);
    }

    private static string? NullIfWhitespace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
