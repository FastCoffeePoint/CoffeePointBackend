using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cpb.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddNameAndChangeUrlNameInCoffeeMachines : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Url",
                table: "CoffeeMachines",
                newName: "Name");

            migrationBuilder.AddColumn<string>(
                name: "MachineHealthCheckEndpointUrl",
                table: "CoffeeMachines",
                type: "character varying(128)",
                maxLength: 128,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MachineHealthCheckEndpointUrl",
                table: "CoffeeMachines");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "CoffeeMachines",
                newName: "Url");
        }
    }
}
