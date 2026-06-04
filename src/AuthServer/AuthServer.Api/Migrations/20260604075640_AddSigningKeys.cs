using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuthServer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddSigningKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SigningKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KeyId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Algorithm = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PrivateKeyPem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicKeyPem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RetiredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SigningKeys", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SigningKeys_KeyId",
                table: "SigningKeys",
                column: "KeyId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SigningKeys");
        }
    }
}
