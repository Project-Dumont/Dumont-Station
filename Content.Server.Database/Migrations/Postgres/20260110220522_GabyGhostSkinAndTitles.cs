// SPDX-FileCopyrightText: 2026 AgentePanela <agentepanela@gmail.com>
// SPDX-FileCopyrightText: 2026 GabyChangelog <agentepanela2@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Content.Server.Database.Migrations.Postgres
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
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ooctitle",
                table: "preference",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "gaby_store_owned_items",
                columns: table => new
                {
                    gaby_store_owned_items_id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    player_id = table.Column<Guid>(type: "uuid", nullable: false),
                    type = table.Column<byte>(type: "smallint", nullable: false),
                    prototype = table.Column<string>(type: "text", nullable: false),
                    purchase_date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
