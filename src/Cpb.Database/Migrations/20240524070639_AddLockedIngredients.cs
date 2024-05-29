using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Cpb.Database.Migrations
{
    /// <inheritdoc />
    public partial class AddLockedIngredients : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LockedIngredients",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    IngredientId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecipeId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockedAmount = table.Column<int>(type: "integer", nullable: false),
                    CreateDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    DeleteDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LockedIngredients", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LockedIngredients_CoffeeRecipes_RecipeId",
                        column: x => x.RecipeId,
                        principalTable: "CoffeeRecipes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockedIngredients_Ingredients_IngredientId",
                        column: x => x.IngredientId,
                        principalTable: "Ingredients",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LockedIngredients_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LockedIngredients_IngredientId",
                table: "LockedIngredients",
                column: "IngredientId");

            migrationBuilder.CreateIndex(
                name: "IX_LockedIngredients_OrderId",
                table: "LockedIngredients",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LockedIngredients_RecipeId",
                table: "LockedIngredients",
                column: "RecipeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LockedIngredients");
        }
    }
}
