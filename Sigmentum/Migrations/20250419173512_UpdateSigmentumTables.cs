using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigmentum.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSigmentumTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "EntryPrice",
                table: "Signals",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPending",
                table: "Signals",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StrategyVersion",
                table: "Signals",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EntryPrice",
                table: "Signals");

            migrationBuilder.DropColumn(
                name: "IsPending",
                table: "Signals");

            migrationBuilder.DropColumn(
                name: "StrategyVersion",
                table: "Signals");
        }
    }
}
