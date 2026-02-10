using Contacts.Api.Dtos.Contacts;
using FluentValidation;

namespace Contacts.Api.Validation;

public sealed class CreateContactRequestValidator : AbstractValidator<CreateContactRequest>
{
    public CreateContactRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Email)
            .MaximumLength(200)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Company)
            .MaximumLength(200)
            .When(x => !string.IsNullOrWhiteSpace(x.Company));

        RuleForEach(x => x.Tags)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.Tags)
            .Must(HaveUniqueTagValues)
            .WithMessage("Tags must be unique.");
    }

    private static bool HaveUniqueTagValues(List<string>? tags)
    {
        if (tags is null || tags.Count == 0)
        {
            return true;
        }

        var normalized = tags.Select(tag => tag.Trim()).ToList();
        return normalized.Distinct(StringComparer.OrdinalIgnoreCase).Count() == normalized.Count;
    }
}
