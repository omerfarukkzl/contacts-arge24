using AutoMapper;
using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Tags;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class TagsController(ContactsDbContext dbContext, IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TagDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TagDto>>> GetTags(CancellationToken cancellationToken)
    {
        var tags = await dbContext.Tags
            .AsNoTracking()
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);

        return Ok(mapper.Map<List<TagDto>>(tags));
    }

    [HttpPost]
    [ProducesResponseType(typeof(TagDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<TagDto>> CreateTag([FromBody] CreateTagRequest request, CancellationToken cancellationToken)
    {
        var normalizedName = request.Name.Trim();

        var duplicateExists = await dbContext.Tags
            .AnyAsync(tag => tag.Name.ToLower() == normalizedName.ToLower(), cancellationToken);

        if (duplicateExists)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate tag",
                Detail = "A tag with the same name already exists."
            });
        }

        var tag = new Tag { Name = normalizedName };
        dbContext.Tags.Add(tag);
        await dbContext.SaveChangesAsync(cancellationToken);

        var mapped = mapper.Map<TagDto>(tag);
        return Created($"/api/tags/{mapped.Id}", mapped);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(int id, CancellationToken cancellationToken)
    {
        var tag = await dbContext.Tags
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
