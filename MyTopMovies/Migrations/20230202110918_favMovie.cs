using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MyTopMovies.Migrations
{
    public partial class favMovie : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FavouriteMovie",
                schema: "AppObj",
                columns: table => new
                {
                    ID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    MovieID = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MovieName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Year = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FavouriteMovie", x => x.ID);
                    table.ForeignKey(
                        name: "FK_FavouriteMovie_Users_UserID",
                        column: x => x.UserID,
                        principalSchema: "UserMngt",
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FavouriteMovie_UserID",
                schema: "AppObj",
                table: "FavouriteMovie",
                column: "UserID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FavouriteMovie",
                schema: "AppObj");
        }
    }
}
