namespace Contacts.Api.Dtos.Contacts;

public sealed class CsvImportResultDto
{
    public int ImportedCount { get; set; }
    public int FailedCount { get; set; }
    public List<CsvRowErrorDto> Errors { get; set; } = [];
}

public sealed class CsvRowErrorDto
{
    public int RowNumber { get; set; }
    public string Field { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RawValue { get; set; }
}
