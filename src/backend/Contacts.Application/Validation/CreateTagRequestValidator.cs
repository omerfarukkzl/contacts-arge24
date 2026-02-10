using Contacts.Api.Dtos.Tags;
using FluentValidation;

namespace Contacts.Api.Validation;

public sealed class CreateTagRequestValidator : AbstractValidator<CreateTagRequest>
{
    public CreateTagRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(50);
    }
}
