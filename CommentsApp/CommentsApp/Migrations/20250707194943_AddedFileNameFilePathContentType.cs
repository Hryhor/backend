using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CommentsApp.Migrations
{
    /// <inheritdoc />
    public partial class AddedFileNameFilePathContentType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContentType",
                table: "Comments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "Comments",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Comments",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContentType",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Comments");
        }
    }
}
