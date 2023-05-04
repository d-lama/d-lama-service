using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class RmLabelNavigtio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TextDataPoints_Labels_LabelId",
                table: "TextDataPoints");

            migrationBuilder.DropForeignKey(
                name: "FK_TextDataPoints_Users_LabelerId",
                table: "TextDataPoints");

            migrationBuilder.DropIndex(
                name: "IX_TextDataPoints_LabelerId",
                table: "TextDataPoints");

            migrationBuilder.DropIndex(
                name: "IX_TextDataPoints_LabelId",
                table: "TextDataPoints");

            migrationBuilder.DropColumn(
                name: "LabelId",
                table: "TextDataPoints");

            migrationBuilder.DropColumn(
                name: "LabelerId",
                table: "TextDataPoints");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LabelId",
                table: "TextDataPoints",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LabelerId",
                table: "TextDataPoints",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TextDataPoints_LabelerId",
                table: "TextDataPoints",
                column: "LabelerId");

            migrationBuilder.CreateIndex(
                name: "IX_TextDataPoints_LabelId",
                table: "TextDataPoints",
                column: "LabelId");

            migrationBuilder.AddForeignKey(
                name: "FK_TextDataPoints_Labels_LabelId",
                table: "TextDataPoints",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TextDataPoints_Users_LabelerId",
                table: "TextDataPoints",
                column: "LabelerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
