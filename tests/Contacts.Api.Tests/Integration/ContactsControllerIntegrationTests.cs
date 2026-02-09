using System.Net;
using System.Net.Http.Json;
using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Tests.Integration;

public sealed class ContactsControllerIntegrationTests : IClassFixture<ContactsApiFactory>
{
    private readonly HttpClient client;

    public ContactsControllerIntegrationTests(ContactsApiFactory factory)
    {
        client = factory.CreateClient();
    }

    [Fact]
    public async Task Should_Create_Then_Get_Contact()
    {
        var createRequest = new CreateContactRequest
        {
            FirstName = "Ali",
            LastName = "Yilmaz",
            Phone = "+905551112233",
            Email = "ali@example.com",
            Company = "ARGE24",
            Notes = "Imported from integration test",
            IsFavorite = true,
            Tags = ["Work", "Team"]
        };

        var createResponse = await client.PostAsJsonAsync("/api/contacts", createRequest);

        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var createdContact = await createResponse.Content.ReadFromJsonAsync<ContactDto>();
        Assert.NotNull(createdContact);

        var getResponse = await client.GetAsync($"/api/contacts/{createdContact.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

        var fetchedContact = await getResponse.Content.ReadFromJsonAsync<ContactDto>();
        Assert.NotNull(fetchedContact);
        Assert.Equal(createRequest.Phone, fetchedContact.Phone);
        Assert.Equal(2, fetchedContact.Tags.Count);
    }

    [Fact]
    public async Task Should_Return_BadRequest_For_Invalid_Email()
    {
        var createRequest = new CreateContactRequest
        {
            FirstName = "Ali",
            LastName = "Yilmaz",
            Phone = "+905551112234",
            Email = "invalid-email"
        };

        var response = await client.PostAsJsonAsync("/api/contacts", createRequest);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
