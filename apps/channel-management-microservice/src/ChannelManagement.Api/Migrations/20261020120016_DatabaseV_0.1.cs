using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChannelManagement.Api.Migrations
{
    /// <inheritdoc />
    public partial class DatabaseV_01 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "newMessageCount",
                table: "Channels",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "newMessageCount",
                table: "Channels");
        }
    }
}
