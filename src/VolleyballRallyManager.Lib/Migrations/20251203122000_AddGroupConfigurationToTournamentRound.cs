using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VolleyballRallyManager.Lib.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupConfigurationToTournamentRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TeamsPerGroup",
                table: "TournamentRounds",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GroupsInRound",
                table: "TournamentRounds",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamsPerGroup",
                table: "TournamentRounds");

            migrationBuilder.DropColumn(
                name: "GroupsInRound",
                table: "TournamentRounds");
        }
    }
}
