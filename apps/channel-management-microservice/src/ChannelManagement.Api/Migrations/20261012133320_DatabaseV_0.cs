using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChannelManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseV_0 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Channels",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    serverId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    categories = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userroles = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    visibility = table.Column<bool>(type: "bit", nullable: false),
                    writerights = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    title = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Channels", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Channels");
        }
    }
}
