using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTopMovies.Migrations
{
    public partial class editSelTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ListName",
                schema: "AppObj",
                table: "MovieSelections",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Count",
                schema: "AppObj",
                table: "MovieSelections",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Count",
                schema: "AppObj",
                table: "MovieSelections");

            migrationBuilder.AlterColumn<string>(
                name: "ListName",
                schema: "AppObj",
                table: "MovieSelections",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
