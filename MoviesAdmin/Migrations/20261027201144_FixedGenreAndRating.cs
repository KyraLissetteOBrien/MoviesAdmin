using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesAdmin.Migrations
{
    /// <inheritdoc />
    public partial class FixedGenreAndRating : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AudienceRating_AudienceRating_AudienceRatingId",
                table: "AudienceRating");

            migrationBuilder.DropForeignKey(
                name: "FK_Genre_Genre_GenreId",
                table: "Genre");

            migrationBuilder.DropIndex(
                name: "IX_Genre_GenreId",
                table: "Genre");

            migrationBuilder.DropIndex(
                name: "IX_AudienceRating_AudienceRatingId",
                table: "AudienceRating");

            migrationBuilder.DropColumn(
                name: "GenreId",
                table: "Genre");

            migrationBuilder.DropColumn(
                name: "AudienceRatingId",
                table: "AudienceRating");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GenreId",
                table: "Genre",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AudienceRatingId",
                table: "AudienceRating",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Genre_GenreId",
                table: "Genre",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_AudienceRating_AudienceRatingId",
                table: "AudienceRating",
                column: "AudienceRatingId");

            migrationBuilder.AddForeignKey(
                name: "FK_AudienceRating_AudienceRating_AudienceRatingId",
                table: "AudienceRating",
                column: "AudienceRatingId",
                principalTable: "AudienceRating",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Genre_Genre_GenreId",
                table: "Genre",
                column: "GenreId",
                principalTable: "Genre",
                principalColumn: "Id");
        }
    }
}
