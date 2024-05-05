using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cpb.Database.Migrations
{
    /// <inheritdoc />
    public partial class CoffeeMachinesAndCoffeeRecipes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CoffeeMachines",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    State = table.Column<int>(type: "integer", nullable: false),
                    Url = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeMachines", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeRecipes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeRecipes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ingredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: true),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ingredients", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeMachineIngredients",
                columns: table => new
                {
                    CoffeeMachineId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeMachineIngredients", x => new { x.CoffeeMachineId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_CoffeeMachineIngredients_CoffeeMachines_CoffeeMachineId",
                        column: x => x.CoffeeMachineId,
                        principalTable: "CoffeeMachines",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoffeeMachineIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CoffeeRecipeIngredients",
                columns: table => new
                {
                    CoffeeRecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CoffeeRecipeIngredients", x => new { x.CoffeeRecipeId, x.IngredientId });
                    table.ForeignKey(
                        name: "FK_CoffeeRecipeIngredients_CoffeeRecipes_CoffeeRecipeId",
                        column: x => x.CoffeeRecipeId,
                        principalTable: "CoffeeRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CoffeeRecipeIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeMachineIngredients_IngredientId",
                table: "CoffeeMachineIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_CoffeeRecipeIngredients_IngredientId",
                table: "CoffeeRecipeIngredients",
                column: "IngredientId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CoffeeMachineIngredients");

            migrationBuilder.DropTable(
                name: "CoffeeRecipeIngredients");

            migrationBuilder.DropTable(
                name: "CoffeeMachines");

            migrationBuilder.DropTable(
                name: "CoffeeRecipes");

            migrationBuilder.DropTable(
                name: "Ingredients");
        }
    }
}
