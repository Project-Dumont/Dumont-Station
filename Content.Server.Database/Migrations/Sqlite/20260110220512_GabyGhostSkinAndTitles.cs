using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Content.Server.Database.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class GabyGhostSkinAndTitles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ghost_skin",
                table: "preference",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ooctitle",
                table: "preference",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gaby_store_owned_items",
                columns: table => new
                {
                    gaby_store_owned_items_id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    player_id = table.Column<Guid>(type: "TEXT", nullable: false),
                    type = table.Column<byte>(type: "INTEGER", nullable: false),
                    prototype = table.Column<string>(type: "TEXT", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gaby_store_owned_items", x => x.gaby_store_owned_items_id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "gaby_store_owned_items");

            migrationBuilder.DropColumn(
                name: "ghost_skin",
                table: "preference");

            migrationBuilder.DropColumn(
                name: "ooctitle",
                table: "preference");
        }
    }
}
