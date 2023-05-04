using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    public partial class AddDataLabelling : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TextDataPoints_Projects_ProjectId",
                table: "TextDataPoints");

            migrationBuilder.DropTable(
                name: "ImageDataPoints");

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

            migrationBuilder.AddColumn<string>(
                name: "Path",
                table: "DataPoint",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_DataPoint",
                table: "DataPoint",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "LabeledDataPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LabelId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DataPointId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabeledDataPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabeledDataPoints_DataPoint_DataPointId",
                        column: x => x.DataPointId,
                        principalTable: "DataPoint",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabeledDataPoints_Labels_LabelId",
                        column: x => x.LabelId,
                        principalTable: "Labels",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_LabeledDataPoints_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabeledDataPoints_DataPointId",
                table: "LabeledDataPoints",
                column: "DataPointId");

            migrationBuilder.CreateIndex(
                name: "IX_LabeledDataPoints_LabelId",
                table: "LabeledDataPoints",
                column: "LabelId");

            migrationBuilder.CreateIndex(
                name: "IX_LabeledDataPoints_UserId",
                table: "LabeledDataPoints",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DataPoint_Projects_ProjectId",
                table: "DataPoint",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DataPoint_Projects_ProjectId",
                table: "DataPoint");

            migrationBuilder.DropTable(
                name: "LabeledDataPoints");

            migrationBuilder.DropPrimaryKey(
                name: "PK_DataPoint",
                table: "DataPoint");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "DataPoint");

            migrationBuilder.DropColumn(
                name: "Path",
                table: "DataPoint");

            migrationBuilder.RenameTable(
                name: "DataPoint",
                newName: "TextDataPoints");

            migrationBuilder.RenameIndex(
                name: "IX_DataPoint_ProjectId",
                table: "TextDataPoints",
                newName: "IX_TextDataPoints_ProjectId");

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
                name: "ImageDataPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "getutcdate()"),
                    DataPointIndex = table.Column<int>(type: "int", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageDataPoints", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_TextDataPoints_Projects_ProjectId",
                table: "TextDataPoints",
                column: "ProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
