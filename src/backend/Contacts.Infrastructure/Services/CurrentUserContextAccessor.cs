using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Contacts.Api.Services;

public sealed class CurrentUserContextAccessor(
    IHttpContextAccessor httpContextAccessor,
    IAppUserService appUserService) : ICurrentUserContextAccessor
{
    public async Task<CurrentUserContext?> GetCurrentAsync(CancellationToken cancellationToken)
    {
        var user = httpContextAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var firebaseUid = user.FindFirst("user_id")?.Value ?? user.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            return null;
        }

        var email = user.FindFirst(ClaimTypes.Email)?.Value ?? user.FindFirst("email")?.Value;
        var displayName = user.FindFirst(ClaimTypes.Name)?.Value ?? user.FindFirst("name")?.Value;

        var appUser = await appUserService.GetOrCreateByFirebaseUidAsync(
            firebaseUid,
            email,
            displayName,
            cancellationToken);

        return new CurrentUserContext(
            appUser.Id,
            appUser.FirebaseUid,
            appUser.Email,
            appUser.DisplayName);
    }
}
