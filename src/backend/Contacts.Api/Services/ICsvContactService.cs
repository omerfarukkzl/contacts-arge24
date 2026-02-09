using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Services;

public interface ICsvContactService
{
    Task<byte[]> ExportAsync(ContactListQuery query, CancellationToken cancellationToken);
    Task<CsvImportResultDto> ImportAsync(IFormFile file, CancellationToken cancellationToken);
}
