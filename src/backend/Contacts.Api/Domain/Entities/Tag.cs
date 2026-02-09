namespace Contacts.Api.Domain.Entities;

public sealed class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public ICollection<ContactTag> ContactTags { get; set; } = new List<ContactTag>();
}
