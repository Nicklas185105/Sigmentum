using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigmentum.Migrations
{
    /// <inheritdoc />
    public partial class AddIndicatorToSignal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Indicator",
                table: "Signals",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Indicator",
                table: "Signals");
        }
    }
}
