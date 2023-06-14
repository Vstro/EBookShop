using Microsoft.EntityFrameworkCore.Migrations;

namespace BookHaven.Data.Migrations
{
    public partial class AddBookCost : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "Carts",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal));

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Books",
                type: "money",
                nullable: false,
                defaultValue: 0m);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Books");

            migrationBuilder.AlterColumn<decimal>(
                name: "Cost",
                table: "Carts",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");
        }
    }
}
