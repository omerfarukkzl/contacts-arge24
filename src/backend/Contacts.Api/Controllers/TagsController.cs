using Contacts.Api.Dtos.Tags;
using Contacts.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TagsController(
    ITagApplicationService tagApplicationService,
    ICurrentUserContextAccessor currentUserContextAccessor) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TagDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetTags(CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var tags = await tagApplicationService.GetTagsAsync(currentUser.AppUserId, cancellationToken);
        return Ok(tags);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var result = await tagApplicationService.CreateTagAsync(currentUser.AppUserId, request, cancellationToken);
        if (result.IsDuplicate)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate tag",
                Detail = "A tag with the same name already exists."
            });
        }

        return Created($"/api/tags/{result.Tag!.Id}", result.Tag);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(int id, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var deleted = await tagApplicationService.DeleteTagAsync(currentUser.AppUserId, id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
