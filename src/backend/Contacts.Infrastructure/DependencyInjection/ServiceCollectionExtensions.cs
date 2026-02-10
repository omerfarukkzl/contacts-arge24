using Contacts.Api.Data;
using Contacts.Api.Repositories;
using Contacts.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Contacts.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ContactsDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IContactRepository, ContactRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        services.AddScoped<IAppUserRepository, AppUserRepository>();

        services.AddScoped<IAppUserService, AppUserService>();
        services.AddScoped<ICurrentUserContextAccessor, CurrentUserContextAccessor>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<ICsvContactService, CsvContactService>();
        services.AddScoped<IContactApplicationService, ContactApplicationService>();
        services.AddScoped<ITagApplicationService, TagApplicationService>();

        return services;
    }
}
