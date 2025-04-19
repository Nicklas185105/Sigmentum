using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Sigmentum.Migrations
{
    /// <inheritdoc />
    public partial class AddIndexForDupError : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Signals_Symbol_Exchange_SignalType_TriggeredAt",
                table: "Signals",
                columns: new[] { "Symbol", "Exchange", "SignalType", "TriggeredAt" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Signals_Symbol_Exchange_SignalType_TriggeredAt",
                table: "Signals");
        }
    }
}
