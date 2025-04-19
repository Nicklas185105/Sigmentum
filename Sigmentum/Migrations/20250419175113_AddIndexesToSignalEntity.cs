using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigmentum.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexesToSignalEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Signals_IsPending",
                table: "Signals",
                column: "IsPending");

            migrationBuilder.CreateIndex(
                name: "IX_Signals_Symbol_TriggeredAt",
                table: "Signals",
                columns: new[] { "Symbol", "TriggeredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Signals_IsPending",
                table: "Signals");

            migrationBuilder.DropIndex(
                name: "IX_Signals_Symbol_TriggeredAt",
                table: "Signals");
        }
    }
}
