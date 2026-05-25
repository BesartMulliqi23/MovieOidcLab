using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AuthServer.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOAuthClients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OAuthClients",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClientName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ClientType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequirePkce = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthClients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OAuthClientRedirectUris",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OAuthClientId = table.Column<int>(type: "int", nullable: false),
                    RedirectUri = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthClientRedirectUris", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthClientRedirectUris_OAuthClients_OAuthClientId",
                        column: x => x.OAuthClientId,
                        principalTable: "OAuthClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OAuthClientScopes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OAuthClientId = table.Column<int>(type: "int", nullable: false),
                    Scope = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OAuthClientScopes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OAuthClientScopes_OAuthClients_OAuthClientId",
                        column: x => x.OAuthClientId,
                        principalTable: "OAuthClients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "OAuthClients",
                columns: new[] { "Id", "ClientId", "ClientName", "ClientType", "CreatedAt", "IsActive", "RequirePkce" },
                values: new object[] { 1, "movies-spa", "Movies SPA", "public", new DateTimeOffset(new DateTime(2026, 5, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), true, true });

            migrationBuilder.InsertData(
                table: "OAuthClientRedirectUris",
                columns: new[] { "Id", "OAuthClientId", "RedirectUri" },
                values: new object[] { 1, 1, "http://localhost:5173/callback" });

            migrationBuilder.InsertData(
                table: "OAuthClientScopes",
                columns: new[] { "Id", "OAuthClientId", "Scope" },
                values: new object[,]
                {
                    { 1, 1, "openid" },
                    { 2, 1, "profile" },
                    { 3, 1, "offline_access" },
                    { 4, 1, "movies.read" },
                    { 5, 1, "movies.write" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_OAuthClientRedirectUris_OAuthClientId_RedirectUri",
                table: "OAuthClientRedirectUris",
                columns: new[] { "OAuthClientId", "RedirectUri" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OAuthClients_ClientId",
                table: "OAuthClients",
                column: "ClientId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OAuthClientScopes_OAuthClientId_Scope",
                table: "OAuthClientScopes",
                columns: new[] { "OAuthClientId", "Scope" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OAuthClientRedirectUris");

            migrationBuilder.DropTable(
                name: "OAuthClientScopes");

            migrationBuilder.DropTable(
                name: "OAuthClients");
        }
    }
}
