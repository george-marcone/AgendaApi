using System;
using CoreFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFlow.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260515_AddUserCreatedAt")]
    public partial class AddUserCreatedAt : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "CreatedAt",
                table: "Users",
                type: "datetimeoffset",
                nullable: false,
                defaultValueSql: "TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00')");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CreatedAt",
                table: "Users",
                column: "CreatedAt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_CreatedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "Users");
        }
    }
}
