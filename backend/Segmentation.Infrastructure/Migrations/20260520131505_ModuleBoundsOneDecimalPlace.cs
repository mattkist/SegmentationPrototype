using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Segmentation.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ModuleBoundsOneDecimalPlace : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "MinimumModule",
                table: "YieldAndScaleRanges",
                type: "TEXT",
                precision: 10,
                scale: 1,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaximumModule",
                table: "YieldAndScaleRanges",
                type: "TEXT",
                precision: 10,
                scale: 1,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MinimumModule",
                table: "YieldAndScaleRanges",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 10,
                oldScale: 1);

            migrationBuilder.AlterColumn<int>(
                name: "MaximumModule",
                table: "YieldAndScaleRanges",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 10,
                oldScale: 1);
        }
    }
}
