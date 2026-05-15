using System;
using CoreFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFlow.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260515_AddUserPasswordHash")]
    public partial class AddUserPasswordHash : Migration
    {
        private const string SeededPasswordHash = "PBKDF2-SHA256.100000.AQIDBAUGBwgJCgsMDQ4PEA==.qcFegJie06o8c1nvLR19oaltyyqxYCeEEOBZYppGVW8=";

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: SeededPasswordHash);

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "Email", "Phone", "PasswordHash", "CreatedAt" },
                values: new object[]
                {
                    new Guid("00000000-0000-0000-0000-000000000101"),
                    "Admin",
                    "admin@coreflow.local",
                    "+5511900000000",
                    SeededPasswordHash,
                    new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero)
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("00000000-0000-0000-0000-000000000101"));

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Users");
        }
    }
}
