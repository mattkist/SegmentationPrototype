using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHasStandardBarn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasStandardBarn",
                table: "TechnologiesKpis",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HasStandardBarnScore",
                table: "SegmentationConfigurationTechnologies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasStandardBarn",
                table: "TechnologiesKpis");

            migrationBuilder.DropColumn(
                name: "HasStandardBarnScore",
                table: "SegmentationConfigurationTechnologies");
        }
    }
}
