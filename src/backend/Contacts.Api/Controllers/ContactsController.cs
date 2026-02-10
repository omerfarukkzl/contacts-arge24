using AutoMapper;
using Contacts.Api.Data;
using Contacts.Api.Domain.Entities;
using Contacts.Api.Dtos.Common;
using Contacts.Api.Dtos.Contacts;
using Contacts.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ContactsController(
    ContactsDbContext dbContext,
    ITagService tagService,
    ICsvContactService csvContactService,
    ICurrentUserContextAccessor currentUserContextAccessor,
    IMapper mapper) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetContacts([FromQuery] ContactListQuery query, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        query.Normalize();

        var filteredQuery = dbContext.Contacts
            .AsNoTracking()
            .Where(contact => contact.OwnerUserId == currentUser.AppUserId)
            .Include(contact => contact.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .ApplyFilters(query);

        var totalCount = await filteredQuery.CountAsync(cancellationToken);

        var contacts = await filteredQuery
            .ApplySorting(query)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var mapped = mapper.Map<List<ContactDto>>(contacts);
        return Ok(new PagedResult<ContactDto>(mapped, query.Page, query.PageSize, totalCount));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> GetContact(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var contact = await dbContext.Contacts
            .AsNoTracking()
            .Where(entity => entity.OwnerUserId == currentUser.AppUserId)
            .Include(entity => entity.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        return Ok(mapper.Map<ContactDto>(contact));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> CreateContact([FromBody] CreateContactRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var normalizedPhone = request.Phone.Trim();
        var duplicatePhoneExists = await dbContext.Contacts
            .AnyAsync(
                contact => contact.OwnerUserId == currentUser.AppUserId && contact.Phone == normalizedPhone,
                cancellationToken);

        if (duplicatePhoneExists)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate phone",
                Detail = "A contact with the same phone number already exists."
            });
        }

        var now = DateTime.UtcNow;
        var contact = new Contact
        {
            OwnerUserId = currentUser.AppUserId,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Phone = normalizedPhone,
            Email = NullIfWhitespace(request.Email),
            Company = NullIfWhitespace(request.Company),
            Notes = NullIfWhitespace(request.Notes),
            IsFavorite = request.IsFavorite,
            CreatedAt = now,
            UpdatedAt = now
        };

        await tagService.SyncContactTagsAsync(contact, currentUser.AppUserId, request.Tags, cancellationToken);

        dbContext.Contacts.Add(contact);
        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(contact)
            .Collection(entity => entity.ContactTags)
            .Query()
            .Include(contactTag => contactTag.Tag)
            .LoadAsync(cancellationToken);

        var mapped = mapper.Map<ContactDto>(contact);
        return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, mapped);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> UpdateContact(Guid id, [FromBody] UpdateContactRequest request, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var contact = await dbContext.Contacts
            .Where(entity => entity.OwnerUserId == currentUser.AppUserId)
            .Include(entity => entity.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        var normalizedPhone = request.Phone.Trim();
        var duplicatePhoneExists = await dbContext.Contacts
            .AnyAsync(
                entity => entity.OwnerUserId == currentUser.AppUserId && entity.Id != id && entity.Phone == normalizedPhone,
                cancellationToken);

        if (duplicatePhoneExists)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate phone",
                Detail = "A contact with the same phone number already exists."
            });
        }

        contact.FirstName = request.FirstName.Trim();
        contact.LastName = request.LastName.Trim();
        contact.Phone = normalizedPhone;
        contact.Email = NullIfWhitespace(request.Email);
        contact.Company = NullIfWhitespace(request.Company);
        contact.Notes = NullIfWhitespace(request.Notes);
        contact.IsFavorite = request.IsFavorite;
        contact.UpdatedAt = DateTime.UtcNow;

        await tagService.SyncContactTagsAsync(contact, currentUser.AppUserId, request.Tags, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);

        await dbContext.Entry(contact)
            .Collection(entity => entity.ContactTags)
            .Query()
            .Include(contactTag => contactTag.Tag)
            .LoadAsync(cancellationToken);

        return Ok(mapper.Map<ContactDto>(contact));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var contact = await dbContext.Contacts
            .Where(entity => entity.OwnerUserId == currentUser.AppUserId)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        contact.IsDeleted = true;
        contact.DeletedAt = DateTime.UtcNow;
        contact.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> RestoreContact(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var contact = await dbContext.Contacts
            .IgnoreQueryFilters()
            .Where(entity => entity.OwnerUserId == currentUser.AppUserId)
            .Include(entity => entity.ContactTags)
            .ThenInclude(contactTag => contactTag.Tag)
            .FirstOrDefaultAsync(entity => entity.Id == id, cancellationToken);

        if (contact is null)
        {
            return NotFound();
        }

        if (!contact.IsDeleted)
        {
            return Ok(mapper.Map<ContactDto>(contact));
        }

        var duplicatePhoneExists = await dbContext.Contacts
            .AnyAsync(
                entity => entity.OwnerUserId == currentUser.AppUserId && entity.Id != id && entity.Phone == contact.Phone,
                cancellationToken);

        if (duplicatePhoneExists)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate phone",
                Detail = "This contact cannot be restored because another active contact uses the same phone number."
            });
        }

        contact.IsDeleted = false;
        contact.DeletedAt = null;
        contact.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return Ok(mapper.Map<ContactDto>(contact));
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportContacts([FromQuery] ContactListQuery query, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var csvFile = await csvContactService.ExportAsync(currentUser.AppUserId, query, cancellationToken);
        var fileName = $"contacts-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(csvFile, "text/csv", fileName);
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(CsvImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CsvImportResultDto>> ImportContacts([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var currentUser = await currentUserContextAccessor.GetCurrentAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        if (file is null)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Missing file",
                Detail = "CSV file is required."
            });
        }

        var result = await csvContactService.ImportAsync(currentUser.AppUserId, file, cancellationToken);
        return Ok(result);
    }

    private static string? NullIfWhitespace(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
