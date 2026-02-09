using Contacts.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Contacts.Api.Tests.Integration;

public sealed class ContactsApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = $"contacts-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ContactsDbContext>>();
            services.RemoveAll<ContactsDbContext>();

            var dbContextConfigDescriptors = services
                .Where(serviceDescriptor => serviceDescriptor.ServiceType == typeof(IDbContextOptionsConfiguration<ContactsDbContext>))
                .ToList();

            foreach (var serviceDescriptor in dbContextConfigDescriptors)
            {
                services.Remove(serviceDescriptor);
            }

            services.AddDbContext<ContactsDbContext>(options =>
                options.UseInMemoryDatabase(databaseName));
        });
    }
}
