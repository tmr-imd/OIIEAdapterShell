using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIRLib.Migrations
{
    /// <inheritdoc />
    public partial class AliasColumnSourceId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_Key",
                table: "PropertyValue",
                column: "Key");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PropertyValue_Key",
                table: "PropertyValue");
        }
    }
}
