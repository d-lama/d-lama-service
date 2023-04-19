using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class DataPoint2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPoint_Labels_LabelId",
                table: "DataPoint");

            migrationBuilder.DropForeignKey(
                name: "FK_DataPoint_Projects_ProjectId",
                table: "DataPoint");

            migrationBuilder.DropForeignKey(
                name: "FK_DataPoint_Users_LabelerId",
                table: "DataPoint");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataPoint",
                table: "DataPoint");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "DataPoint");

            migrationBuilder.RenameTable(
                name: "DataPoint",
                newName: "TextDataPoints");

            migrationBuilder.RenameIndex(
                name: "IX_DataPoint_ProjectId",
                table: "TextDataPoints",
                newName: "IX_TextDataPoints_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_DataPoint_LabelId",
                table: "TextDataPoints",
                newName: "IX_TextDataPoints_LabelId");

            migrationBuilder.RenameIndex(
                name: "IX_DataPoint_LabelerId",
                table: "TextDataPoints",
                newName: "IX_TextDataPoints_LabelerId");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "TextDataPoints",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "TextDataPoints",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_TextDataPoints",
                table: "TextDataPoints",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ImageDataPoint",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataPointIndex = table.Column<int>(type: "int", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageDataPoint", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TextDataPoints_Labels_LabelId",
                table: "TextDataPoints",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_TextDataPoints_Projects_ProjectId",
                table: "TextDataPoints",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TextDataPoints_Users_LabelerId",
                table: "TextDataPoints",
                column: "LabelerId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TextDataPoints_Labels_LabelId",
                table: "TextDataPoints");

            migrationBuilder.DropForeignKey(
                name: "FK_TextDataPoints_Projects_ProjectId",
                table: "TextDataPoints");

            migrationBuilder.DropForeignKey(
                name: "FK_TextDataPoints_Users_LabelerId",
                table: "TextDataPoints");

            migrationBuilder.DropTable(
                name: "ImageDataPoint");

            migrationBuilder.DropPrimaryKey(
                name: "PK_TextDataPoints",
                table: "TextDataPoints");

            migrationBuilder.RenameTable(
                name: "TextDataPoints",
                newName: "DataPoint");

            migrationBuilder.RenameIndex(
                name: "IX_TextDataPoints_ProjectId",
                table: "DataPoint",
                newName: "IX_DataPoint_ProjectId");

            migrationBuilder.RenameIndex(
                name: "IX_TextDataPoints_LabelId",
                table: "DataPoint",
                newName: "IX_DataPoint_LabelId");

            migrationBuilder.RenameIndex(
                name: "IX_TextDataPoints_LabelerId",
                table: "DataPoint",
                newName: "IX_DataPoint_LabelerId");

            migrationBuilder.AlterColumn<int>(
                name: "ProjectId",
                table: "DataPoint",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "DataPoint",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "DataPoint",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataPoint",
                table: "DataPoint",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPoint_Labels_LabelId",
                table: "DataPoint",
                column: "LabelId",
                principalTable: "Labels",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPoint_Projects_ProjectId",
                table: "DataPoint",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DataPoint_Users_LabelerId",
                table: "DataPoint",
                column: "LabelerId",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
