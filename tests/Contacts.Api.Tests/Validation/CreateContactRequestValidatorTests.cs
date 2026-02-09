using Contacts.Api.Dtos.Contacts;
using Contacts.Api.Validation;

namespace Contacts.Api.Tests.Validation;

public sealed class CreateContactRequestValidatorTests
{
    private readonly CreateContactRequestValidator validator = new();

    [Fact]
    public void Should_Fail_When_FirstName_Is_Empty()
    {
        var request = new CreateContactRequest
        {
            FirstName = string.Empty,
            LastName = "Doe",
            Phone = "+905551112233"
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.FirstName));
    }

    [Fact]
    public void Should_Fail_When_Email_Is_Invalid()
    {
        var request = new CreateContactRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Phone = "+905551112233",
            Email = "invalid-email"
        };

        var result = validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == nameof(CreateContactRequest.Email));
    }

    [Fact]
    public void Should_Pass_With_Valid_Request()
    {
        var request = new CreateContactRequest
        {
            FirstName = "Jane",
            LastName = "Doe",
            Phone = "+905551112233",
            Email = "jane@example.com",
            Company = "Acme",
            Tags = ["Family", "Work"]
        };

        var result = validator.Validate(request);

        Assert.True(result.IsValid);
    }
}
