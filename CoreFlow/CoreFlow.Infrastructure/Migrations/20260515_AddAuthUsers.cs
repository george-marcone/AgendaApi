using System;
using CoreFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFlow.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260515_AddAuthUsers")]
    public partial class AddAuthUsers : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthUsers", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuthUsers_Email",
                table: "AuthUsers",
                column: "Email",
                unique: true);

            migrationBuilder.InsertData(
                table: "AuthUsers",
                columns: new[] { "Id", "Name", "Email", "PasswordHash", "CreatedAt" },
                values: new object[]
                {
                    new Guid("00000000-0000-0000-0000-000000000101"),
                    "Admin",
                    "admin@coreflow.local",
                    "PBKDF2-SHA256.100000.AQIDBAUGBwgJCgsMDQ4PEA==.qcFegJie06o8c1nvLR19oaltyyqxYCeEEOBZYppGVW8=",
                    new DateTimeOffset(2026, 5, 15, 0, 0, 0, TimeSpan.Zero)
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AuthUsers");
        }
    }
}
