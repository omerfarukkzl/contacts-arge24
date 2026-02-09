using Contacts.Api.Data;
using Contacts.Api.Infrastructure;
using Contacts.Api.Services;
using Contacts.Api.Validation;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<ContactsDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateContactRequestValidator>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICsvContactService, CsvContactService>();
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

app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
app.UseCors("Frontend");
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
