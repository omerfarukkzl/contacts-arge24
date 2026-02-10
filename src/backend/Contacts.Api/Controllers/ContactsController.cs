using Contacts.Api.Dtos.Common;
using Contacts.Api.Dtos.Contacts;
using Contacts.Api.Exceptions;
using Contacts.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Contacts.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public sealed class ContactsController(
    IContactApplicationService contactApplicationService,
    ICurrentUserContextAccessor currentUserContextAccessor) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<ContactDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<PagedResult<ContactDto>>> GetContacts(
        [FromQuery] ContactListQuery query,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var result = await contactApplicationService.GetContactsAsync(currentUser.AppUserId, query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactDto>> GetContact(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var contact = await contactApplicationService.GetContactAsync(currentUser.AppUserId, id, cancellationToken);
        return contact is null ? NotFound() : Ok(contact);
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> CreateContact(
        [FromBody] CreateContactRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            var contact = await contactApplicationService.CreateContactAsync(
                currentUser.AppUserId,
                request,
                cancellationToken);

            return CreatedAtAction(nameof(GetContact), new { id = contact.Id }, contact);
        }
        catch (DuplicatePhoneException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate phone",
                Detail = "A contact with the same phone number already exists."
            });
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> UpdateContact(
        Guid id,
        [FromBody] UpdateContactRequest request,
        CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        try
        {
            var contact = await contactApplicationService.UpdateContactAsync(
                currentUser.AppUserId,
                id,
                request,
                cancellationToken);

            return contact is null ? NotFound() : Ok(contact);
        }
        catch (DuplicatePhoneException)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate phone",
                Detail = "A contact with the same phone number already exists."
            });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteContact(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var deleted = await contactApplicationService.DeleteContactAsync(currentUser.AppUserId, id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }

    [HttpPost("{id:guid}/restore")]
    [ProducesResponseType(typeof(ContactDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ContactDto>> RestoreContact(Guid id, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var result = await contactApplicationService.RestoreContactAsync(currentUser.AppUserId, id, cancellationToken);

        if (!result.IsFound)
        {
            return NotFound();
        }

        if (result.HasConflict)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Duplicate phone",
                Detail = "This contact cannot be restored because another active contact uses the same phone number."
            });
        }

        return Ok(result.Contact);
    }

    [HttpGet("export")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportContacts([FromQuery] ContactListQuery query, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var csvFile = await contactApplicationService.ExportCsvAsync(currentUser.AppUserId, query, cancellationToken);
        var fileName = $"contacts-{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return File(csvFile, "text/csv", fileName);
    }

    [HttpGet("export/excel")]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ExportContactsExcel([FromQuery] ContactListQuery query, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
        if (currentUser is null)
        {
            return Unauthorized();
        }

        var excelFile = await contactApplicationService.ExportExcelAsync(currentUser.AppUserId, query, cancellationToken);
        var fileName = $"contacts-{DateTime.UtcNow:yyyyMMddHHmmss}.xls";

        return File(
            excelFile,
            "application/vnd.ms-excel",
            fileName);
    }

    [HttpPost("import")]
    [ProducesResponseType(typeof(CsvImportResultDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<CsvImportResultDto>> ImportContacts([FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        var currentUser = await GetCurrentUserAsync(cancellationToken);
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

        await using var stream = file.OpenReadStream();
        var result = await contactApplicationService.ImportCsvAsync(
            currentUser.AppUserId,
            stream,
            file.Length,
            cancellationToken);
        return Ok(result);
    }

    private Task<CurrentUserContext?> GetCurrentUserAsync(CancellationToken cancellationToken)
    {
        return currentUserContextAccessor.GetCurrentAsync(cancellationToken);
    }
}
