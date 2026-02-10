using Contacts.Api.Dtos.Auth;
using Contacts.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.Api.Controllers;

[ApiController]
[Route("api/auth")]
[Authorize]
public sealed class AuthController(ICurrentUserContextAccessor currentUserContextAccessor) : ControllerBase
{
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthMeDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthMeDto>> Me(CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        return Ok(new AuthMeDto
        {
            Id = currentUser.AppUserId,
            FirebaseUid = currentUser.FirebaseUid,
            Email = currentUser.Email,
            DisplayName = currentUser.DisplayName
        });
    }
}
