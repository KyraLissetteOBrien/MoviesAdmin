using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MoviesAdmin.Migrations
{
    /// <inheritdoc />
    public partial class AddCriticReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CriticReview_AudienceRating_AudienceRatingId",
                table: "CriticReview");

            migrationBuilder.DropForeignKey(
                name: "FK_CriticReview_Genre_GenreId",
                table: "CriticReview");

            migrationBuilder.DropForeignKey(
                name: "FK_CriticReview_Movie_MovieId",
                table: "CriticReview");

            migrationBuilder.DropIndex(
                name: "IX_CriticReview_AudienceRatingId",
                table: "CriticReview");

            migrationBuilder.DropIndex(
                name: "IX_CriticReview_GenreId",
                table: "CriticReview");

            migrationBuilder.DropColumn(
                name: "AudienceRatingId",
                table: "CriticReview");

            migrationBuilder.RenameColumn(
                name: "GenreId",
                table: "CriticReview",
                newName: "Rating");

            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "CriticReview",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CriticReview",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_CriticReview_Movie_MovieId",
                table: "CriticReview",
                column: "MovieId",
                principalTable: "Movie",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CriticReview_Movie_MovieId",
                table: "CriticReview");

            migrationBuilder.RenameColumn(
                name: "Rating",
                table: "CriticReview",
                newName: "GenreId");

            migrationBuilder.AlterColumn<int>(
                name: "MovieId",
                table: "CriticReview",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "CriticReview",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AddColumn<int>(
                name: "AudienceRatingId",
                table: "CriticReview",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_CriticReview_AudienceRatingId",
                table: "CriticReview",
                column: "AudienceRatingId");

            migrationBuilder.CreateIndex(
                name: "IX_CriticReview_GenreId",
                table: "CriticReview",
                column: "GenreId");

            migrationBuilder.AddForeignKey(
                name: "FK_CriticReview_AudienceRating_AudienceRatingId",
                table: "CriticReview",
                column: "AudienceRatingId",
                principalTable: "AudienceRating",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CriticReview_Genre_GenreId",
                table: "CriticReview",
                column: "GenreId",
                principalTable: "Genre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CriticReview_Movie_MovieId",
                table: "CriticReview",
                column: "MovieId",
                principalTable: "Movie",
                principalColumn: "Id");
        }
    }
}
