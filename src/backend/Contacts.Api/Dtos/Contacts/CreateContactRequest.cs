namespace Contacts.Api.Dtos.Contacts;

public sealed class CreateContactRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Company { get; set; }
    public string? Notes { get; set; }
    public bool IsFavorite { get; set; }
    public List<string> Tags { get; set; } = [];
}
