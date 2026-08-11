using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nuxiba.Api.Migrations
{
    /// <inheritdoc />
    public partial class FixAreaNameColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Area",
                table: "ccRIACat_Areas",
                newName: "AreaName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AreaName",
                table: "ccRIACat_Areas",
                newName: "Area");
        }
    }
}
