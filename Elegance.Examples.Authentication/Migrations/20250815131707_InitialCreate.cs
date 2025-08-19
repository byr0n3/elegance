using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elegance.Examples.Authentication.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    username = table.Column<string>(type: "TEXT", nullable: false),
                    password = table.Column<byte[]>(type: "BLOB", nullable: false),
                    security_stamp = table.Column<string>(type: "TEXT", nullable: true),
                    last_sign_in_timestamp = table.Column<long>(type: "INTEGER", nullable: true),
                    access_failed_count = table.Column<int>(type: "INTEGER", nullable: false),
                    access_lockout_end = table.Column<long>(type: "INTEGER", nullable: true),
                    mfa = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_users_username",
                table: "users",
                column: "username",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "users");
        }
    }
}
