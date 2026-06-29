using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ToDoApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _002NewAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TodoItems",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "TodoItems",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111112"));

            migrationBuilder.DeleteData(
                table: "TodoItems",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111113"));

            migrationBuilder.DeleteData(
                table: "TodoItems",
                keyColumn: "Id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111114"));

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "TodoItems",
                newName: "LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "TodoItems",
                newName: "CreatedAtUtc");

            migrationBuilder.RenameColumn(
                name: "UpdatedAt",
                table: "Tags",
                newName: "LastModifiedAtUtc");

            migrationBuilder.RenameColumn(
                name: "CreatedAt",
                table: "Tags",
                newName: "CreatedAtUtc");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "TodoItems",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Tags",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "TodoItems");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Tags");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Tags");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAtUtc",
                table: "TodoItems",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "TodoItems",
                newName: "CreatedAt");

            migrationBuilder.RenameColumn(
                name: "LastModifiedAtUtc",
                table: "Tags",
                newName: "UpdatedAt");

            migrationBuilder.RenameColumn(
                name: "CreatedAtUtc",
                table: "Tags",
                newName: "CreatedAt");

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "TodoItems",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Tags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.InsertData(
                table: "TodoItems",
                columns: new[] { "Id", "CreatedAt", "Description", "DueDate", "IsCompleted", "IsDeleted", "Level", "ParentTodoId", "Title", "UpdatedAt", "UserId" },
                values: new object[,]
                {
                    { new Guid("11111111-1111-1111-1111-111111111111"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Set up decoupled solution boundaries separating UI, Infrastructure, and Core.", new DateTime(2026, 6, 15, 2, 0, 0, 0, DateTimeKind.Local), false, false, 2, null, "Configure Clean Architecture", null, new Guid("11111111-1111-1111-1111-111111111115") },
                    { new Guid("11111111-1111-1111-1111-111111111112"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Establish index patterns and seed initial testing database tables.", new DateTime(2026, 6, 20, 2, 0, 0, 0, DateTimeKind.Local), true, false, 1, null, "Database Design & Optimization", null, new Guid("11111111-1111-1111-1111-111111111115") },
                    { new Guid("11111111-1111-1111-1111-111111111113"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Add basic SqlServer and Tools dependencies to the Infrastructure project layer.", new DateTime(2026, 6, 25, 2, 0, 0, 0, DateTimeKind.Local), false, false, 2, null, "Install Entity Framework Core Packages", null, new Guid("11111111-1111-1111-1111-111111111115") },
                    { new Guid("11111111-1111-1111-1111-111111111114"), new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Write custom configurations for table constraints and relationship keys.", new DateTime(2026, 6, 30, 2, 0, 0, 0, DateTimeKind.Local), false, false, 0, null, "Configure Fluent API Mappings", null, new Guid("11111111-1111-1111-1111-111111111115") }
                });
        }
    }
}
