using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddGameSaveHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Characters_StoryNodes_CurrentStoryNodeId",
                table: "Characters");

            migrationBuilder.DropForeignKey(
                name: "FK_Characters_User_UserId",
                table: "Characters");

            migrationBuilder.DropIndex(
                name: "IX_Characters_CurrentStoryNodeId",
                table: "Characters");

            migrationBuilder.DropIndex(
                name: "IX_Characters_UserId",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "DateCreated",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "DateUpdated",
                table: "Characters");

            migrationBuilder.AddColumn<int>(
                name: "CurrentDialogueIndex",
                table: "GameSaves",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LastChoiceId",
                table: "GameSaves",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VisitedNodeIds",
                table: "GameSaves",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentDialogueIndex",
                table: "GameSaves");

            migrationBuilder.DropColumn(
                name: "LastChoiceId",
                table: "GameSaves");

            migrationBuilder.DropColumn(
                name: "VisitedNodeIds",
                table: "GameSaves");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateCreated",
                table: "Characters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DateUpdated",
                table: "Characters",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Characters_CurrentStoryNodeId",
                table: "Characters",
                column: "CurrentStoryNodeId");

            migrationBuilder.CreateIndex(
                name: "IX_Characters_UserId",
                table: "Characters",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_StoryNodes_CurrentStoryNodeId",
                table: "Characters",
                column: "CurrentStoryNodeId",
                principalTable: "StoryNodes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Characters_User_UserId",
                table: "Characters",
                column: "UserId",
                principalTable: "User",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
