namespace Contacts.Api.Dtos.Contacts;

public sealed class ContactListQuery
{
    private const int DefaultPageSize = 20;
    private const int MaxPageSize = 100;

    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = DefaultPageSize;
    public string SortBy { get; set; } = "firstName";
    public string SortDir { get; set; } = "asc";
    public bool FavoriteOnly { get; set; }
    public string? Tag { get; set; }
    public string? Company { get; set; }

    public void Normalize()
    {
        if (Page < 1)
        {
            Page = 1;
        }

        if (PageSize < 1)
        {
            PageSize = DefaultPageSize;
        }

        if (PageSize > MaxPageSize)
        {
            PageSize = MaxPageSize;
        }

        SortBy = (SortBy ?? "firstName").Trim();
        SortDir = (SortDir ?? "asc").Trim().ToLowerInvariant();

        if (SortDir is not ("asc" or "desc"))
        {
            SortDir = "asc";
        }
    }
}
