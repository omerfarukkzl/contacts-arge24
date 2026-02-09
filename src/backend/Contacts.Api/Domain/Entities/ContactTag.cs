namespace Contacts.Api.Domain.Entities;

public sealed class ContactTag
{
    public Guid ContactId { get; set; }
    public int TagId { get; set; }
    public Contact Contact { get; set; } = null!;
    public Tag Tag { get; set; } = null!;
}
