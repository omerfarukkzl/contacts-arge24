namespace Contacts.Api.Services;

public sealed record CurrentUserContext(
    Guid AppUserId,
    string FirebaseUid,
    string? Email,
    string? DisplayName);
