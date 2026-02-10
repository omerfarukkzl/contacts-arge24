using Contacts.Api.Domain.Entities;
using Contacts.Api.Repositories;

namespace Contacts.Api.Services;

public sealed class AppUserService(
    IAppUserRepository appUserRepository,
    IUnitOfWork unitOfWork) : IAppUserService
{
    public async Task<AppUser> GetOrCreateByFirebaseUidAsync(
        string firebaseUid,
        string? email,
        string? displayName,
        CancellationToken cancellationToken)
    {
        var normalizedUid = firebaseUid.Trim();
        if (string.IsNullOrWhiteSpace(normalizedUid))
        {
            throw new InvalidOperationException("Firebase UID claim is missing.");
        }

        var normalizedEmail = NullIfWhitespace(email);
        var normalizedDisplayName = NullIfWhitespace(displayName);
        var now = DateTime.UtcNow;

        var appUser = await appUserRepository.GetByFirebaseUidAsync(normalizedUid, cancellationToken);

        if (appUser is null)
        {
            appUser = new AppUser
            {
                FirebaseUid = normalizedUid,
                Email = normalizedEmail,
                DisplayName = normalizedDisplayName,
                CreatedAtUtc = now,
                LastSeenAtUtc = now
            };

            await appUserRepository.AddAsync(appUser, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return appUser;
        }

        var shouldPersist = false;

        if (appUser.Email != normalizedEmail)
        {
            appUser.Email = normalizedEmail;
            shouldPersist = true;
        }

        if (appUser.DisplayName != normalizedDisplayName)
        {
            appUser.DisplayName = normalizedDisplayName;
            shouldPersist = true;
        }

        if (appUser.LastSeenAtUtc <= now.AddMinutes(-5))
        {
            appUser.LastSeenAtUtc = now;
            shouldPersist = true;
        }

        if (shouldPersist)
        {
            appUserRepository.Update(appUser);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return appUser;
    }

    private static string? NullIfWhitespace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
