using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPriceRubToCars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PriceRub",
                table: "Cars",
                type: "numeric",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceRub",
                table: "Cars");
        }
    }
}
