using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CIRLib.Migrations
{
    /// <inheritdoc />
    public partial class CIRLibInitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistryId = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registry", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Category",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    RegistryRefId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistryId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Category", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Category_Registry_RegistryRefId",
                        column: x => x.RegistryRefId,
                        principalTable: "Registry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Entry",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    IdInSource = table.Column<string>(type: "TEXT", nullable: false),
                    SourceId = table.Column<string>(type: "TEXT", nullable: false),
                    CIRId = table.Column<string>(type: "TEXT", nullable: false),
                    SourceOwnerId = table.Column<string>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    Inactive = table.Column<bool>(type: "INTEGER", nullable: false),
                    CategoryRefId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RegistryRefId = table.Column<Guid>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<string>(type: "TEXT", nullable: false),
                    RegistryId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Entry_Category_CategoryRefId",
                        column: x => x.CategoryRefId,
                        principalTable: "Category",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Entry_Registry_RegistryRefId",
                        column: x => x.RegistryRefId,
                        principalTable: "Registry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Property",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyId = table.Column<string>(type: "TEXT", nullable: false),
                    PropertyValue = table.Column<string>(type: "TEXT", nullable: false),
                    DataType = table.Column<string>(type: "TEXT", nullable: false),
                    EntryRefId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntryIdInSource = table.Column<string>(type: "TEXT", nullable: false),
                    CategoryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    RegistryId = table.Column<Guid>(type: "TEXT", nullable: true),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Property", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Property_Category_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Category",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Property_Entry_EntryRefId",
                        column: x => x.EntryRefId,
                        principalTable: "Entry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Property_Registry_RegistryId",
                        column: x => x.RegistryId,
                        principalTable: "Registry",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PropertyValue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false),
                    UnitOfMeasure = table.Column<string>(type: "TEXT", nullable: false),
                    PropertyRefId = table.Column<Guid>(type: "TEXT", nullable: false),
                    PropertyId = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateCreated = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedBy = table.Column<string>(type: "TEXT", nullable: false),
                    DateModified = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropertyValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PropertyValue_Property_PropertyRefId",
                        column: x => x.PropertyRefId,
                        principalTable: "Property",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Category_CategoryId",
                table: "Category",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Category_RegistryRefId",
                table: "Category",
                column: "RegistryRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Entry_CategoryRefId",
                table: "Entry",
                column: "CategoryRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Entry_IdInSource",
                table: "Entry",
                column: "IdInSource");

            migrationBuilder.CreateIndex(
                name: "IX_Entry_RegistryRefId",
                table: "Entry",
                column: "RegistryRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_CategoryId",
                table: "Property",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_EntryRefId",
                table: "Property",
                column: "EntryRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_PropertyId",
                table: "Property",
                column: "PropertyId");

            migrationBuilder.CreateIndex(
                name: "IX_Property_RegistryId",
                table: "Property",
                column: "RegistryId");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_Key",
                table: "PropertyValue",
                column: "Key");

            migrationBuilder.CreateIndex(
                name: "IX_PropertyValue_PropertyRefId",
                table: "PropertyValue",
                column: "PropertyRefId");

            migrationBuilder.CreateIndex(
                name: "IX_Registry_RegistryId",
                table: "Registry",
                column: "RegistryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PropertyValue");

            migrationBuilder.DropTable(
                name: "Property");

            migrationBuilder.DropTable(
                name: "Entry");

            migrationBuilder.DropTable(
                name: "Category");

            migrationBuilder.DropTable(
                name: "Registry");
        }
    }
}
