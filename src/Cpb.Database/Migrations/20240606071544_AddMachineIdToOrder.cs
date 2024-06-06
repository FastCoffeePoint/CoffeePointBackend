using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cpb.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddMachineIdToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "MachineId",
                table: "Orders",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_MachineId",
                table: "Orders",
                column: "MachineId");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_CoffeeMachines_MachineId",
                table: "Orders",
                column: "MachineId",
                principalTable: "CoffeeMachines",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Orders_CoffeeMachines_MachineId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_MachineId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "MachineId",
                table: "Orders");
        }
    }
}
