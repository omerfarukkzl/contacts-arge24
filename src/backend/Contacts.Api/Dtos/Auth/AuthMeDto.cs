namespace Contacts.Api.Dtos.Auth;

public sealed class AuthMeDto
{
    public Guid Id { get; init; }
    public string FirebaseUid { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? DisplayName { get; init; }
}
