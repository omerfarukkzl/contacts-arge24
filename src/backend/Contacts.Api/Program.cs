using Contacts.Api.Data;
using Contacts.Api.DependencyInjection;
using Contacts.Api.Infrastructure;
using Contacts.Api.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

var firebaseProjectId = builder.Configuration["Authentication:Firebase:ProjectId"];
var firebaseIssuer = builder.Configuration["Authentication:Firebase:Issuer"];

if (string.IsNullOrWhiteSpace(firebaseProjectId))
{
    firebaseProjectId = "replace-with-firebase-project-id";
}

if (string.IsNullOrWhiteSpace(firebaseIssuer))
{
    firebaseIssuer = $"https://securetoken.google.com/{firebaseProjectId}";
}

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateContactRequestValidator>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.MapInboundClaims = false;
        options.Authority = $"https://securetoken.google.com/{firebaseProjectId}";
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = firebaseIssuer,
            ValidateAudience = true,
            ValidAudience = firebaseProjectId,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            NameClaimType = "name",
            RoleClaimType = "role"
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseMiddleware<RequestLoggingMiddleware>();
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapGet("/", () => "Contacts API is running!");

var shouldPrepareDatabase = builder.Environment.IsDevelopment() ||
                            builder.Configuration.GetValue<bool>("Database:ApplyMigrationsOnStartup");

if (shouldPrepareDatabase)
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ContactsDbContext>();
    if (dbContext.Database.GetMigrations().Any())
    {
        dbContext.Database.Migrate();
    }
    else
    {
        // If no migration exists yet, create schema directly for local development flow.
        dbContext.Database.EnsureCreated();
    }
}

app.Run();

public partial class Program;
