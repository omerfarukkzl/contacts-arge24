using System.Globalization;
using System.Text;
using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Contacts;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Services;

public sealed class CsvContactService(
    ContactsDbContext dbContext,
    IValidator<CreateContactRequest> createValidator,
    ITagService tagService) : ICsvContactService
{
    public async Task<byte[]> ExportAsync(ContactListQuery query, CancellationToken cancellationToken)
    {
        query.Normalize();

        var contacts = await dbContext.Contacts
            .AsNoTracking()
            .Include(contact => contact.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .ApplyFilters(query)
            .ApplySorting(query)
            .ToListAsync(cancellationToken);

        using var writer = new StringWriter(CultureInfo.InvariantCulture);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        csv.WriteField("FirstName");
        csv.WriteField("LastName");
        csv.WriteField("Phone");
        csv.WriteField("Email");
        csv.WriteField("Company");
        csv.WriteField("Notes");
        csv.WriteField("IsFavorite");
        csv.WriteField("Tags");
        await csv.NextRecordAsync();

        foreach (var contact in contacts)
        {
            csv.WriteField(contact.FirstName);
            csv.WriteField(contact.LastName);
            csv.WriteField(contact.Phone);
            csv.WriteField(contact.Email);
            csv.WriteField(contact.Company);
            csv.WriteField(contact.Notes);
            csv.WriteField(contact.IsFavorite);
            csv.WriteField(string.Join(';', contact.ContactTags.Select(contactTag => contactTag.Tag.Name)));
            await csv.NextRecordAsync();
        }

        await writer.FlushAsync();
        return Encoding.UTF8.GetBytes(writer.ToString());
    }

    public async Task<CsvImportResultDto> ImportAsync(IFormFile file, CancellationToken cancellationToken)
    {
        var result = new CsvImportResultDto();

        if (file.Length == 0)
        {
            result.Errors.Add(new CsvRowErrorDto
            {
                RowNumber = 0,
                Field = "file",
                Message = "CSV file is empty."
            });
            result.FailedCount = 1;
            return result;
        }

        await using var stream = file.OpenReadStream();
        using var reader = new StreamReader(stream);

        var csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            TrimOptions = TrimOptions.Trim,
            PrepareHeaderForMatch = arguments => arguments.Header?.Trim().ToLowerInvariant() ?? string.Empty,
            HeaderValidated = null,
            MissingFieldFound = null,
            BadDataFound = null
        };

        using var csv = new CsvReader(reader, csvConfiguration);

        if (!await csv.ReadAsync())
        {
            result.Errors.Add(new CsvRowErrorDto
            {
                RowNumber = 0,
                Field = "file",
                Message = "CSV header is missing."
            });
            result.FailedCount = 1;
            return result;
        }

        try
        {
            csv.ReadHeader();
        }
        catch (Exception exception)
        {
            result.Errors.Add(new CsvRowErrorDto
            {
                RowNumber = 0,
                Field = "file",
                Message = $"CSV header is invalid: {exception.Message}"
            });
            result.FailedCount = 1;
            return result;
        }

        while (await csv.ReadAsync())
        {
            var rowNumber = csv.Context.Parser.Row;

            CsvContactRow? row;
            try
            {
                row = csv.GetRecord<CsvContactRow>();
            }
            catch (Exception exception)
            {
                result.Errors.Add(new CsvRowErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "row",
                    Message = $"Unable to parse row: {exception.Message}"
                });
                continue;
            }

            if (!TryParseBoolean(row.IsFavorite, out var isFavorite))
            {
                result.Errors.Add(new CsvRowErrorDto
                {
                    RowNumber = rowNumber,
                    Field = "isFavorite",
                    Message = "Invalid boolean value.",
                    RawValue = row.IsFavorite
                });
                continue;
            }

            var request = new CreateContactRequest
            {
                FirstName = row.FirstName?.Trim() ?? string.Empty,
                LastName = row.LastName?.Trim() ?? string.Empty,
                Phone = row.Phone?.Trim() ?? string.Empty,
                Email = NullIfWhitespace(row.Email),
                Company = NullIfWhitespace(row.Company),
                Notes = NullIfWhitespace(row.Notes),
                IsFavorite = isFavorite,
                Tags = ParseTags(row.Tags)
            };

            var validationResult = await createValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                foreach (var validationError in validationResult.Errors)
                {
                    result.Errors.Add(new CsvRowErrorDto
                    {
                        RowNumber = rowNumber,
                        Field = validationError.PropertyName,
                        Message = validationError.ErrorMessage,
                        RawValue = GetRawFieldValue(row, validationError.PropertyName)
                    });
                }

                continue;
            }

            var duplicatePhoneExists = await dbContext.Contacts
                .AsNoTracking()
                .AnyAsync(contact => contact.Phone == request.Phone, cancellationToken);

            if (duplicatePhoneExists)
            {
                result.Errors.Add(new CsvRowErrorDto
                {
                    RowNumber = rowNumber,
                    Field = nameof(CreateContactRequest.Phone),
                    Message = "Phone number already exists.",
                    RawValue = request.Phone
                });
                continue;
            }

            var contact = new Contact
            {
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Phone = request.Phone.Trim(),
                Email = NullIfWhitespace(request.Email),
                Company = NullIfWhitespace(request.Company),
                Notes = request.Notes,
                IsFavorite = request.IsFavorite,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await tagService.SyncContactTagsAsync(contact, request.Tags, cancellationToken);

            dbContext.Contacts.Add(contact);
            await dbContext.SaveChangesAsync(cancellationToken);
            result.ImportedCount++;
        }

        result.FailedCount = result.Errors
            .Select(error => error.RowNumber)
            .Distinct()
            .Count();

        return result;
    }

    private static string? NullIfWhitespace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static bool TryParseBoolean(string? rawValue, out bool parsed)
    {
        parsed = false;

        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return true;
        }

        var value = rawValue.Trim();
        if (bool.TryParse(value, out parsed))
        {
            return true;
        }

        if (value is "1" or "yes" or "y")
        {
            parsed = true;
            return true;
        }

        if (value is "0" or "no" or "n")
        {
            parsed = false;
            return true;
        }

        return false;
    }

    private static List<string> ParseTags(string? rawTags)
    {
        if (string.IsNullOrWhiteSpace(rawTags))
        {
            return [];
        }

        return rawTags.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private static string? GetRawFieldValue(CsvContactRow row, string field)
    {
        return field switch
        {
            nameof(CreateContactRequest.FirstName) => row.FirstName,
            nameof(CreateContactRequest.LastName) => row.LastName,
            nameof(CreateContactRequest.Phone) => row.Phone,
            nameof(CreateContactRequest.Email) => row.Email,
            nameof(CreateContactRequest.Company) => row.Company,
            nameof(CreateContactRequest.Notes) => row.Notes,
            nameof(CreateContactRequest.IsFavorite) => row.IsFavorite,
            nameof(CreateContactRequest.Tags) => row.Tags,
            _ => null
        };
    }

    private sealed class CsvContactRow
    {
        [Name("firstname")]
        public string? FirstName { get; set; }

        [Name("lastname")]
        public string? LastName { get; set; }

        [Name("phone")]
        public string? Phone { get; set; }

        [Name("email")]
        public string? Email { get; set; }

        [Name("company")]
        public string? Company { get; set; }

        [Name("notes")]
        public string? Notes { get; set; }

        [Name("isfavorite")]
        public string? IsFavorite { get; set; }

        [Name("tags")]
        public string? Tags { get; set; }
    }
}
