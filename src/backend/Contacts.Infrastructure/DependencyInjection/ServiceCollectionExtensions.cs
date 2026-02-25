using Contacts.Api.Data;
using Contacts.Api.Repositories;
using Contacts.Api.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace Contacts.Api.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = ResolvePostgresConnectionString(configuration);

        services.AddDbContext<ContactsDbContext>(options =>
            options.UseNpgsql(connectionString));

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

    private static string ResolvePostgresConnectionString(IConfiguration configuration)
    {
        var rawConnectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(rawConnectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");
        }

        if (!rawConnectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase) &&
            !rawConnectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase))
        {
            return rawConnectionString;
        }

        return ConvertPostgresUrlToConnectionString(rawConnectionString);
    }

    private static string ConvertPostgresUrlToConnectionString(string postgresUrl)
    {
        var uri = new Uri(postgresUrl);
        var userInfoParts = uri.UserInfo.Split(':', 2, StringSplitOptions.None);
        var username = userInfoParts.Length > 0 ? Uri.UnescapeDataString(userInfoParts[0]) : string.Empty;
        var password = userInfoParts.Length > 1 ? Uri.UnescapeDataString(userInfoParts[1]) : string.Empty;

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = uri.Host,
            Port = uri.IsDefaultPort ? 5432 : uri.Port,
            Database = uri.AbsolutePath.TrimStart('/'),
            Username = username,
            Password = password
        };

        var query = uri.Query.TrimStart('?');
        if (!string.IsNullOrWhiteSpace(query))
        {
            var pairs = query.Split('&', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in pairs)
            {
                var parts = pair.Split('=', 2, StringSplitOptions.None);
                if (parts.Length != 2)
                {
                    continue;
                }

                var key = Uri.UnescapeDataString(parts[0]);
                var value = Uri.UnescapeDataString(parts[1]);

                if (key.Equals("sslmode", StringComparison.OrdinalIgnoreCase))
                {
                    if (Enum.TryParse<SslMode>(value, true, out var sslMode))
                    {
                        builder.SslMode = sslMode;
                    }
                    continue;
                }

                if (key.Equals("trust server certificate", StringComparison.OrdinalIgnoreCase) ||
                    key.Equals("trustservercertificate", StringComparison.OrdinalIgnoreCase))
                {
                    if (bool.TryParse(value, out var trustServerCertificate))
                    {
                        builder.TrustServerCertificate = trustServerCertificate;
                    }
                }
            }
        }

        return builder.ConnectionString;
    }
}
