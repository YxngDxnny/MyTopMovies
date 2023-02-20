using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTopMovies.Migrations
{
    public partial class releaseDate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ReleaseDate",
                schema: "AppObj",
                table: "MovieChoices",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReleaseDate",
                schema: "AppObj",
                table: "FavouriteMovie",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                schema: "AppObj",
                table: "MovieChoices");

            migrationBuilder.DropColumn(
                name: "ReleaseDate",
                schema: "AppObj",
                table: "FavouriteMovie");
        }
    }
}
