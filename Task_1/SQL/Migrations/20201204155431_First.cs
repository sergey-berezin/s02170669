using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SQL.Migrations
{
    public partial class First : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImageClasses",
                columns: table => new
                {
                    ImageClassId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ClassName = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageClasses", x => x.ImageClassId);
                });

            migrationBuilder.CreateTable(
                name: "ImageFile",
                columns: table => new
                {
                    ImageFileId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Img = table.Column<byte[]>(type: "BLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageFile", x => x.ImageFileId);
                });

            migrationBuilder.CreateTable(
                name: "Images",
                columns: table => new
                {
                    ImageInfoId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ImageName = table.Column<string>(type: "TEXT", nullable: true),
                    ClassName = table.Column<string>(type: "TEXT", nullable: true),
                    Prob = table.Column<float>(type: "REAL", nullable: false),
                    NumOfRequests = table.Column<int>(type: "INTEGER", nullable: false),
                    ImageHash = table.Column<string>(type: "TEXT", nullable: true),
                    ByteImageImageFileId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Images", x => x.ImageInfoId);
                    table.ForeignKey(
                        name: "FK_Images_ImageFile_ByteImageImageFileId",
                        column: x => x.ByteImageImageFileId,
                        principalTable: "ImageFile",
                        principalColumn: "ImageFileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ImageClassImageInfo",
                columns: table => new
                {
                    ImageClassesImageClassId = table.Column<int>(type: "INTEGER", nullable: false),
                    ImagesImageInfoId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImageClassImageInfo", x => new { x.ImageClassesImageClassId, x.ImagesImageInfoId });
                    table.ForeignKey(
                        name: "FK_ImageClassImageInfo_ImageClasses_ImageClassesImageClassId",
                        column: x => x.ImageClassesImageClassId,
                        principalTable: "ImageClasses",
                        principalColumn: "ImageClassId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ImageClassImageInfo_Images_ImagesImageInfoId",
                        column: x => x.ImagesImageInfoId,
                        principalTable: "Images",
                        principalColumn: "ImageInfoId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ImageClassImageInfo_ImagesImageInfoId",
                table: "ImageClassImageInfo",
                column: "ImagesImageInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_Images_ByteImageImageFileId",
                table: "Images",
                column: "ByteImageImageFileId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImageClassImageInfo");

            migrationBuilder.DropTable(
                name: "ImageClasses");

            migrationBuilder.DropTable(
                name: "Images");

            migrationBuilder.DropTable(
                name: "ImageFile");
        }
    }
}
