using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Services;

public interface ICsvContactService
{
    Task<byte[]> ExportAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<CsvImportResultDto> ImportAsync(Guid ownerUserId, IFormFile file, CancellationToken cancellationToken);
}
