using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class Project5 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "LabelSets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ProjectId",
                table: "DataPointSets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_LabelSets_ProjectId",
                table: "LabelSets",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_DataPointSets_ProjectId",
                table: "DataPointSets",
                column: "ProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPointSets_Projects_ProjectId",
                table: "DataPointSets",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LabelSets_Projects_ProjectId",
                table: "LabelSets",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPointSets_Projects_ProjectId",
                table: "DataPointSets");

            migrationBuilder.DropForeignKey(
                name: "FK_LabelSets_Projects_ProjectId",
                table: "LabelSets");

            migrationBuilder.DropIndex(
                name: "IX_LabelSets_ProjectId",
                table: "LabelSets");

            migrationBuilder.DropIndex(
                name: "IX_DataPointSets_ProjectId",
                table: "DataPointSets");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "LabelSets");

            migrationBuilder.DropColumn(
                name: "ProjectId",
                table: "DataPointSets");
        }
    }
}
