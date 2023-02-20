using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTopMovies.Migrations
{
    public partial class listfilter : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenreID",
                schema: "AppObj",
                table: "MovieLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearFilter1",
                schema: "AppObj",
                table: "MovieLists",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "YearFilter2",
                schema: "AppObj",
                table: "MovieLists",
                type: "int",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GenreID",
                schema: "AppObj",
                table: "MovieLists");

            migrationBuilder.DropColumn(
                name: "YearFilter1",
                schema: "AppObj",
                table: "MovieLists");

            migrationBuilder.DropColumn(
                name: "YearFilter2",
                schema: "AppObj",
                table: "MovieLists");
        }
    }
}
