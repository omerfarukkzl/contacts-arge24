using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Contacts.Api.Dtos.Auth;
using Contacts.Api.Dtos.Contacts;

namespace Contacts.Api.Tests.Integration;

public sealed class ContactsControllerIntegrationTests : IClassFixture<ContactsApiFactory>
{
    private readonly HttpClient client;
    private readonly ContactsApiFactory factory;

    public ContactsControllerIntegrationTests(ContactsApiFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName, "user-a");
    }

    [Fact]
    public async Task Should_Return_Unauthorized_Without_Authentication()
    {
        using var anonymousClient = factory.CreateClient();

        var response = await anonymousClient.GetAsync("/api/contacts");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
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
    public async Task Should_Hide_Another_Users_Contact_With_NotFound()
    {
        var createRequest = new CreateContactRequest
        {
            FirstName = "Ayse",
            LastName = "Yildiz",
            Phone = "+905551112290"
        };

        var createResponse = await client.PostAsJsonAsync("/api/contacts", createRequest);
        var createdContact = await createResponse.Content.ReadFromJsonAsync<ContactDto>();
        Assert.NotNull(createdContact);

        using var secondUserClient = factory.CreateClient();
        secondUserClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(TestAuthHandler.SchemeName, "user-b");

        var secondUserResponse = await secondUserClient.GetAsync($"/api/contacts/{createdContact.Id}");

        Assert.Equal(HttpStatusCode.NotFound, secondUserResponse.StatusCode);
    }

    [Fact]
    public async Task Should_Provision_User_On_Auth_Me()
    {
        var response = await client.GetAsync("/api/auth/me");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var me = await response.Content.ReadFromJsonAsync<AuthMeDto>();
        Assert.NotNull(me);
        Assert.NotEqual(Guid.Empty, me.Id);
        Assert.Equal("user-a", me.FirebaseUid);
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

    [Fact]
    public async Task Should_Export_Contacts_As_Excel()
    {
        var createRequest = new CreateContactRequest
        {
            FirstName = "Export",
            LastName = "User",
            Phone = "+905551112299"
        };

        var createResponse = await client.PostAsJsonAsync("/api/contacts", createRequest);
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var response = await client.GetAsync("/api/contacts/export/excel");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(
            "application/vnd.ms-excel",
            response.Content.Headers.ContentType?.MediaType);

        var fileBytes = await response.Content.ReadAsByteArrayAsync();
        Assert.True(fileBytes.Length > 0);
    }
}
