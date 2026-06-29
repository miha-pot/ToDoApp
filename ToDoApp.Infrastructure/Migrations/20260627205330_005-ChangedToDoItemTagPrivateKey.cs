using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ToDoApp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class _005ChangedToDoItemTagPrivateKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoItemTags",
                table: "TodoItemTags");

            migrationBuilder.DropIndex(
                name: "IX_TodoItemTags_TodoItemId",
                table: "TodoItemTags");

            migrationBuilder.DropColumn(
                name: "Id",
                table: "TodoItemTags");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Tags",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoItemTags",
                table: "TodoItemTags",
                columns: new[] { "TodoItemId", "TagId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_TodoItemTags",
                table: "TodoItemTags");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Tags");

            migrationBuilder.AddColumn<Guid>(
                name: "Id",
                table: "TodoItemTags",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_TodoItemTags",
                table: "TodoItemTags",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItemTags_TodoItemId",
                table: "TodoItemTags",
                column: "TodoItemId");
        }
    }
}
