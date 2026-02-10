using Contacts.Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Contacts.Api.Data;

public sealed class ContactsDbContext(DbContextOptions<ContactsDbContext> options) : DbContext(options)
{
    public DbSet<AppUser> AppUsers => Set<AppUser>();
    public DbSet<Contact> Contacts => Set<Contact>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<ContactTag> ContactTags => Set<ContactTag>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("AppUsers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.FirebaseUid).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(256);
            entity.Property(x => x.DisplayName).HasMaxLength(256);
            entity.Property(x => x.CreatedAtUtc).IsRequired();
            entity.Property(x => x.LastSeenAtUtc).IsRequired();
            entity.HasIndex(x => x.FirebaseUid)
                .IsUnique()
                .HasDatabaseName("IX_AppUsers_FirebaseUid");
        });

        modelBuilder.Entity<Contact>(entity =>
        {
            entity.ToTable("Contacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Phone).HasMaxLength(30).IsRequired();
            entity.Property(x => x.Email).HasMaxLength(200);
            entity.Property(x => x.Company).HasMaxLength(200);
            entity.Property(x => x.IsFavorite).HasDefaultValue(false);
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).IsRequired();
            entity.HasIndex(x => new { x.OwnerUserId, x.Phone })
                .IsUnique()
                .HasDatabaseName("IX_Contacts_OwnerUserId_Phone")
                .HasFilter("[IsDeleted] = 0");
            entity.HasIndex(x => x.OwnerUserId).HasDatabaseName("IX_Contacts_OwnerUserId");
            entity.HasIndex(x => x.IsDeleted).HasDatabaseName("IX_Contacts_IsDeleted");
            entity.HasIndex(x => x.IsFavorite).HasDatabaseName("IX_Contacts_IsFavorite");
            entity.HasOne(x => x.OwnerUser)
                .WithMany(x => x.Contacts)
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.ToTable("Tags");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OwnerUserId).IsRequired();
            entity.Property(x => x.Name).HasMaxLength(50).IsRequired();
            entity.HasIndex(x => new { x.OwnerUserId, x.Name })
                .IsUnique()
                .HasDatabaseName("IX_Tags_OwnerUserId_Name");
            entity.HasIndex(x => x.OwnerUserId).HasDatabaseName("IX_Tags_OwnerUserId");
            entity.HasOne(x => x.OwnerUser)
                .WithMany(x => x.Tags)
                .HasForeignKey(x => x.OwnerUserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<ContactTag>(entity =>
        {
            entity.ToTable("ContactTags");
            entity.HasKey(x => new { x.ContactId, x.TagId });

            entity.HasOne(x => x.Contact)
                .WithMany(x => x.ContactTags)
                .HasForeignKey(x => x.ContactId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(x => x.Tag)
                .WithMany(x => x.ContactTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        base.OnModelCreating(modelBuilder);
    }
}
