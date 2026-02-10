namespace Contacts.Api.Services;

public interface ICurrentUserContextAccessor
{
    Task<CurrentUserContext?> GetCurrentAsync(CancellationToken cancellationToken);
}
