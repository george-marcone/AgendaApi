using System;
using CoreFlow.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreFlow.Infrastructure.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260514_InitialCreate")]
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            // Seed 50 users
            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Name", "Email", "Phone" },
                values: new object[,]
                {
                    { new Guid("00000000-0000-0000-0000-000000000001"), "User 1", "user1@example.com", "+55 11 900001" },
                    { new Guid("00000000-0000-0000-0000-000000000002"), "User 2", "user2@example.com", "+55 11 900002" },
                    { new Guid("00000000-0000-0000-0000-000000000003"), "User 3", "user3@example.com", "+55 11 900003" },
                    { new Guid("00000000-0000-0000-0000-000000000004"), "User 4", "user4@example.com", "+55 11 900004" },
                    { new Guid("00000000-0000-0000-0000-000000000005"), "User 5", "user5@example.com", "+55 11 900005" },
                    { new Guid("00000000-0000-0000-0000-000000000006"), "User 6", "user6@example.com", "+55 11 900006" },
                    { new Guid("00000000-0000-0000-0000-000000000007"), "User 7", "user7@example.com", "+55 11 900007" },
                    { new Guid("00000000-0000-0000-0000-000000000008"), "User 8", "user8@example.com", "+55 11 900008" },
                    { new Guid("00000000-0000-0000-0000-000000000009"), "User 9", "user9@example.com", "+55 11 900009" },
                    { new Guid("00000000-0000-0000-0000-00000000000a"), "User 10", "user10@example.com", "+55 11 900010" },
                    { new Guid("00000000-0000-0000-0000-00000000000b"), "User 11", "user11@example.com", "+55 11 900011" },
                    { new Guid("00000000-0000-0000-0000-00000000000c"), "User 12", "user12@example.com", "+55 11 900012" },
                    { new Guid("00000000-0000-0000-0000-00000000000d"), "User 13", "user13@example.com", "+55 11 900013" },
                    { new Guid("00000000-0000-0000-0000-00000000000e"), "User 14", "user14@example.com", "+55 11 900014" },
                    { new Guid("00000000-0000-0000-0000-00000000000f"), "User 15", "user15@example.com", "+55 11 900015" },
                    { new Guid("00000000-0000-0000-0000-000000000010"), "User 16", "user16@example.com", "+55 11 900016" },
                    { new Guid("00000000-0000-0000-0000-000000000011"), "User 17", "user17@example.com", "+55 11 900017" },
                    { new Guid("00000000-0000-0000-0000-000000000012"), "User 18", "user18@example.com", "+55 11 900018" },
                    { new Guid("00000000-0000-0000-0000-000000000013"), "User 19", "user19@example.com", "+55 11 900019" },
                    { new Guid("00000000-0000-0000-0000-000000000014"), "User 20", "user20@example.com", "+55 11 900020" },
                    { new Guid("00000000-0000-0000-0000-000000000015"), "User 21", "user21@example.com", "+55 11 900021" },
                    { new Guid("00000000-0000-0000-0000-000000000016"), "User 22", "user22@example.com", "+55 11 900022" },
                    { new Guid("00000000-0000-0000-0000-000000000017"), "User 23", "user23@example.com", "+55 11 900023" },
                    { new Guid("00000000-0000-0000-0000-000000000018"), "User 24", "user24@example.com", "+55 11 900024" },
                    { new Guid("00000000-0000-0000-0000-000000000019"), "User 25", "user25@example.com", "+55 11 900025" },
                    { new Guid("00000000-0000-0000-0000-00000000001a"), "User 26", "user26@example.com", "+55 11 900026" },
                    { new Guid("00000000-0000-0000-0000-00000000001b"), "User 27", "user27@example.com", "+55 11 900027" },
                    { new Guid("00000000-0000-0000-0000-00000000001c"), "User 28", "user28@example.com", "+55 11 900028" },
                    { new Guid("00000000-0000-0000-0000-00000000001d"), "User 29", "user29@example.com", "+55 11 900029" },
                    { new Guid("00000000-0000-0000-0000-00000000001e"), "User 30", "user30@example.com", "+55 11 900030" },
                    { new Guid("00000000-0000-0000-0000-00000000001f"), "User 31", "user31@example.com", "+55 11 900031" },
                    { new Guid("00000000-0000-0000-0000-000000000020"), "User 32", "user32@example.com", "+55 11 900032" },
                    { new Guid("00000000-0000-0000-0000-000000000021"), "User 33", "user33@example.com", "+55 11 900033" },
                    { new Guid("00000000-0000-0000-0000-000000000022"), "User 34", "user34@example.com", "+55 11 900034" },
                    { new Guid("00000000-0000-0000-0000-000000000023"), "User 35", "user35@example.com", "+55 11 900035" },
                    { new Guid("00000000-0000-0000-0000-000000000024"), "User 36", "user36@example.com", "+55 11 900036" },
                    { new Guid("00000000-0000-0000-0000-000000000025"), "User 37", "user37@example.com", "+55 11 900037" },
                    { new Guid("00000000-0000-0000-0000-000000000026"), "User 38", "user38@example.com", "+55 11 900038" },
                    { new Guid("00000000-0000-0000-0000-000000000027"), "User 39", "user39@example.com", "+55 11 900039" },
                    { new Guid("00000000-0000-0000-0000-000000000028"), "User 40", "user40@example.com", "+55 11 900040" },
                    { new Guid("00000000-0000-0000-0000-000000000029"), "User 41", "user41@example.com", "+55 11 900041" },
                    { new Guid("00000000-0000-0000-0000-00000000002a"), "User 42", "user42@example.com", "+55 11 900042" },
                    { new Guid("00000000-0000-0000-0000-00000000002b"), "User 43", "user43@example.com", "+55 11 900043" },
                    { new Guid("00000000-0000-0000-0000-00000000002c"), "User 44", "user44@example.com", "+55 11 900044" },
                    { new Guid("00000000-0000-0000-0000-00000000002d"), "User 45", "user45@example.com", "+55 11 900045" },
                    { new Guid("00000000-0000-0000-0000-00000000002e"), "User 46", "user46@example.com", "+55 11 900046" },
                    { new Guid("00000000-0000-0000-0000-00000000002f"), "User 47", "user47@example.com", "+55 11 900047" },
                    { new Guid("00000000-0000-0000-0000-000000000030"), "User 48", "user48@example.com", "+55 11 900048" },
                    { new Guid("00000000-0000-0000-0000-000000000031"), "User 49", "user49@example.com", "+55 11 900049" },
                    { new Guid("00000000-0000-0000-0000-000000000032"), "User 50", "user50@example.com", "+55 11 900050" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
