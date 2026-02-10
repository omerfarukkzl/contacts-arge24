using Contacts.Api.Dtos.Common;
using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Services;

public interface IContactApplicationService
{
    Task<PagedResult<ContactDto>> GetContactsAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<ContactDto?> GetContactAsync(Guid ownerUserId, Guid contactId, CancellationToken cancellationToken);
    Task<ContactDto> CreateContactAsync(Guid ownerUserId, CreateContactRequest request, CancellationToken cancellationToken);
    Task<ContactDto?> UpdateContactAsync(Guid ownerUserId, Guid contactId, UpdateContactRequest request, CancellationToken cancellationToken);
    Task<bool> DeleteContactAsync(Guid ownerUserId, Guid contactId, CancellationToken cancellationToken);
    Task<RestoreContactResult> RestoreContactAsync(Guid ownerUserId, Guid contactId, CancellationToken cancellationToken);
    Task<byte[]> ExportCsvAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<byte[]> ExportExcelAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<CsvImportResultDto> ImportCsvAsync(
        Guid ownerUserId,
        Stream stream,
        long fileLength,
        CancellationToken cancellationToken);
}

public sealed record RestoreContactResult(
    bool IsFound,
    bool HasConflict,
    ContactDto? Contact);
