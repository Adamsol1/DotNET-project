using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class mergeFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Characters");

            migrationBuilder.AddColumn<string>(
                name: "AmbientSoundUrl",
                table: "StoryNodes",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BackgroundMusicUrl",
                table: "StoryNodes",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Health",
                table: "GameSaves",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AudioUrl",
                table: "Choices",
                type: "TEXT",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "HealthEffect",
                table: "Choices",
                type: "INTEGER",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmbientSoundUrl",
                table: "StoryNodes");

            migrationBuilder.DropColumn(
                name: "BackgroundMusicUrl",
                table: "StoryNodes");

            migrationBuilder.DropColumn(
                name: "Health",
                table: "GameSaves");

            migrationBuilder.DropColumn(
                name: "AudioUrl",
                table: "Choices");

            migrationBuilder.DropColumn(
                name: "HealthEffect",
                table: "Choices");

            migrationBuilder.AddColumn<int>(
                name: "UserId",
                table: "Characters",
                type: "INTEGER",
                nullable: true);
        }
    }
}
