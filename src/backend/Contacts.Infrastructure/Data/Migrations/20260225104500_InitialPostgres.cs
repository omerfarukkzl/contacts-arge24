using System;
using Contacts.Api.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Contacts.Api.Data.Migrations;

[DbContext(typeof(ContactsDbContext))]
[Migration("20260225104500_InitialPostgres")]
public partial class InitialPostgres : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "AppUsers",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                FirebaseUid = table.Column<string>(maxLength: 128, nullable: false),
                Email = table.Column<string>(maxLength: 256, nullable: true),
                DisplayName = table.Column<string>(maxLength: 256, nullable: true),
                CreatedAtUtc = table.Column<DateTime>(nullable: false),
                LastSeenAtUtc = table.Column<DateTime>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_AppUsers", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "Contacts",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                OwnerUserId = table.Column<Guid>(nullable: false),
                FirstName = table.Column<string>(maxLength: 100, nullable: false),
                LastName = table.Column<string>(maxLength: 100, nullable: false),
                Phone = table.Column<string>(maxLength: 30, nullable: false),
                Email = table.Column<string>(maxLength: 200, nullable: true),
                Company = table.Column<string>(maxLength: 200, nullable: true),
                Notes = table.Column<string>(nullable: true),
                IsFavorite = table.Column<bool>(nullable: false, defaultValue: false),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true),
                IsDeleted = table.Column<bool>(nullable: false, defaultValue: false),
                DeletedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Contacts", x => x.Id);
                table.ForeignKey(
                    name: "FK_Contacts_AppUsers_OwnerUserId",
                    column: x => x.OwnerUserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "Tags",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                OwnerUserId = table.Column<Guid>(nullable: false),
                Name = table.Column<string>(maxLength: 50, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Tags", x => x.Id);
                table.ForeignKey(
                    name: "FK_Tags_AppUsers_OwnerUserId",
                    column: x => x.OwnerUserId,
                    principalTable: "AppUsers",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.NoAction);
            });

        migrationBuilder.CreateTable(
            name: "ContactTags",
            columns: table => new
            {
                ContactId = table.Column<Guid>(nullable: false),
                TagId = table.Column<int>(nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_ContactTags", x => new { x.ContactId, x.TagId });
                table.ForeignKey(
                    name: "FK_ContactTags_Contacts_ContactId",
                    column: x => x.ContactId,
                    principalTable: "Contacts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_ContactTags_Tags_TagId",
                    column: x => x.TagId,
                    principalTable: "Tags",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_AppUsers_FirebaseUid",
            table: "AppUsers",
            column: "FirebaseUid",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_Contacts_IsDeleted",
            table: "Contacts",
            column: "IsDeleted");

        migrationBuilder.CreateIndex(
            name: "IX_Contacts_IsFavorite",
            table: "Contacts",
            column: "IsFavorite");

        migrationBuilder.CreateIndex(
            name: "IX_Contacts_OwnerUserId",
            table: "Contacts",
            column: "OwnerUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Contacts_OwnerUserId_Phone",
            table: "Contacts",
            columns: new[] { "OwnerUserId", "Phone" },
            unique: true,
            filter: "\"IsDeleted\" = false");

        migrationBuilder.CreateIndex(
            name: "IX_ContactTags_TagId",
            table: "ContactTags",
            column: "TagId");

        migrationBuilder.CreateIndex(
            name: "IX_Tags_OwnerUserId",
            table: "Tags",
            column: "OwnerUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Tags_OwnerUserId_Name",
            table: "Tags",
            columns: new[] { "OwnerUserId", "Name" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "ContactTags");

        migrationBuilder.DropTable(
            name: "Contacts");

        migrationBuilder.DropTable(
            name: "Tags");

        migrationBuilder.DropTable(
            name: "AppUsers");
    }
}
