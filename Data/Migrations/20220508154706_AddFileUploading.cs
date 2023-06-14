using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace BookHaven.Data.Migrations
{
    public partial class AddFileUploading : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Base64Files",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    FileName = table.Column<string>(nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BookId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Base64Files", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Base64Files_Books_BookId",
                        column: x => x.BookId,
                        principalTable: "Books",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Base64Files_BookId",
                table: "Base64Files",
                column: "BookId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Base64Files");
        }
    }
}
