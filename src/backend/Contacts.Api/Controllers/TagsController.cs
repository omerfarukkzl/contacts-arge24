using AutoMapper;
using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Tags;
using Contacts.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class TagsController(
    ContactsDbContext dbContext,
    ICurrentUserContextAccessor currentUserContextAccessor,
    IMapper mapper) : ControllerBase
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

        var tags = await dbContext.Tags
            .AsNoTracking()
            .Where(tag => tag.OwnerUserId == currentUser.AppUserId)
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);

        return Ok(mapper.Map<List<TagDto>>(tags));
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

        var normalizedName = request.Name.Trim();

        var duplicateExists = await dbContext.Tags
            .AnyAsync(
                tag => tag.OwnerUserId == currentUser.AppUserId && tag.Name.ToLower() == normalizedName.ToLower(),
                cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate tag",
                Detail = "A tag with the same name already exists."
            });
        }

        var tag = new Tag
        {
            OwnerUserId = currentUser.AppUserId,
            Name = normalizedName
        };

        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);

        var mapped = mapper.Map<TagDto>(tag);
        return Created($"/api/tags/{mapped.Id}", mapped);
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

        var tag = await dbContext.Tags
            .Where(entity => entity.OwnerUserId == currentUser.AppUserId)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (tag is null)
        {
            return NotFound();
        }

        dbContext.Tags.Remove(tag);
        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }
}
