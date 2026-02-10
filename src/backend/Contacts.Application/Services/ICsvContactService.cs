using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Services;

public interface ICsvContactService
{
    Task<byte[]> ExportCsvAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<byte[]> ExportExcelAsync(Guid ownerUserId, ContactListQuery query, CancellationToken cancellationToken);
    Task<CsvImportResultDto> ImportAsync(
        Guid ownerUserId,
        Stream stream,
        long fileLength,
        CancellationToken cancellationToken);
}
