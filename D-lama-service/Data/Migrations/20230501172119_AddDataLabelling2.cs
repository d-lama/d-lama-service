using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class AddDataLabelling2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPoint_Projects_ProjectId",
                table: "DataPoint");

            migrationBuilder.DropForeignKey(
                name: "FK_LabeledDataPoints_DataPoint_DataPointId",
                table: "LabeledDataPoints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataPoint",
                table: "DataPoint");

            migrationBuilder.RenameTable(
                name: "DataPoint",
                newName: "DataPoints");

            migrationBuilder.RenameIndex(
                name: "IX_DataPoint_ProjectId",
                table: "DataPoints",
                newName: "IX_DataPoints_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataPoints",
                table: "DataPoints",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPoints_Projects_ProjectId",
                table: "DataPoints",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LabeledDataPoints_DataPoints_DataPointId",
                table: "LabeledDataPoints",
                column: "DataPointId",
                principalTable: "DataPoints",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPoints_Projects_ProjectId",
                table: "DataPoints");

            migrationBuilder.DropForeignKey(
                name: "FK_LabeledDataPoints_DataPoints_DataPointId",
                table: "LabeledDataPoints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataPoints",
                table: "DataPoints");

            migrationBuilder.RenameTable(
                name: "DataPoints",
                newName: "DataPoint");

            migrationBuilder.RenameIndex(
                name: "IX_DataPoints_ProjectId",
                table: "DataPoint",
                newName: "IX_DataPoint_ProjectId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataPoint",
                table: "DataPoint",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPoint_Projects_ProjectId",
                table: "DataPoint",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LabeledDataPoints_DataPoint_DataPointId",
                table: "LabeledDataPoints",
                column: "DataPointId",
                principalTable: "DataPoint",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
