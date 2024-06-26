﻿// <auto-generated />
using System;
using Cpb.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Cpb.Database.Migrations
{
    [DbContext(typeof(DbCoffeePointContext))]
    [Migration("20240606071544_AddMachineIdToOrder")]
    partial class AddMachineIdToOrder
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeMachine", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("MachineHealthCheckEndpointUrl")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("CoffeeMachines");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeMachineIngredient", b =>
                {
                    b.Property<Guid>("CoffeeMachineId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("IngredientId")
                        .HasColumnType("uuid");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.HasKey("CoffeeMachineId", "IngredientId");

                    b.HasIndex("IngredientId");

                    b.ToTable("CoffeeMachineIngredients");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeRecipe", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("CurrentOrdersCount")
                        .HasColumnType("integer");

                    b.Property<DateTimeOffset?>("DeleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.ToTable("CoffeeRecipes");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeRecipeIngredient", b =>
                {
                    b.Property<Guid>("CoffeeRecipeId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("IngredientId")
                        .HasColumnType("uuid");

                    b.Property<int>("Amount")
                        .HasColumnType("integer");

                    b.HasKey("CoffeeRecipeId", "IngredientId");

                    b.HasIndex("IngredientId");

                    b.ToTable("CoffeeRecipeIngredients");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbIngredient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Name")
                        .HasMaxLength(128)
                        .HasColumnType("character varying(128)");

                    b.HasKey("Id");

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbLockedIngredient", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("IngredientId")
                        .HasColumnType("uuid");

                    b.Property<int>("LockedAmount")
                        .HasColumnType("integer");

                    b.Property<Guid>("OrderId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RecipeId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("IngredientId");

                    b.HasIndex("OrderId");

                    b.HasIndex("RecipeId");

                    b.ToTable("LockedIngredients");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbOrder", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("CoffeeRecipeId")
                        .HasColumnType("uuid");

                    b.Property<DateTimeOffset>("CreateDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTimeOffset?>("DeleteDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("MachineId")
                        .HasColumnType("uuid");

                    b.Property<int>("State")
                        .HasColumnType("integer");

                    b.Property<Guid>("UserId")
                        .HasColumnType("uuid");

                    b.HasKey("Id");

                    b.HasIndex("CoffeeRecipeId");

                    b.HasIndex("MachineId");

                    b.HasIndex("UserId");

                    b.ToTable("Orders");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbUser", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .HasMaxLength(320)
                        .HasColumnType("character varying(320)");

                    b.Property<string>("FirstName")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<string>("HashedPassword")
                        .HasColumnType("text");

                    b.Property<string>("LastName")
                        .HasMaxLength(64)
                        .HasColumnType("character varying(64)");

                    b.Property<int>("Role")
                        .HasMaxLength(32)
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeMachineIngredient", b =>
                {
                    b.HasOne("Cpb.Database.Entities.DbCoffeeMachine", "CoffeeMachine")
                        .WithMany("Links")
                        .HasForeignKey("CoffeeMachineId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Cpb.Database.Entities.DbIngredient", "Ingredient")
                        .WithMany()
                        .HasForeignKey("IngredientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CoffeeMachine");

                    b.Navigation("Ingredient");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeRecipeIngredient", b =>
                {
                    b.HasOne("Cpb.Database.Entities.DbCoffeeRecipe", "CoffeeRecipe")
                        .WithMany("Links")
                        .HasForeignKey("CoffeeRecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Cpb.Database.Entities.DbIngredient", "Ingredient")
                        .WithMany("Links")
                        .HasForeignKey("IngredientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CoffeeRecipe");

                    b.Navigation("Ingredient");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbLockedIngredient", b =>
                {
                    b.HasOne("Cpb.Database.Entities.DbIngredient", "Ingredient")
                        .WithMany()
                        .HasForeignKey("IngredientId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Cpb.Database.Entities.DbOrder", "Order")
                        .WithMany("LockedIngredients")
                        .HasForeignKey("OrderId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Cpb.Database.Entities.DbCoffeeRecipe", "Recipe")
                        .WithMany()
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Ingredient");

                    b.Navigation("Order");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbOrder", b =>
                {
                    b.HasOne("Cpb.Database.Entities.DbCoffeeRecipe", "CoffeeRecipe")
                        .WithMany()
                        .HasForeignKey("CoffeeRecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Cpb.Database.Entities.DbCoffeeMachine", "Machine")
                        .WithMany("Orders")
                        .HasForeignKey("MachineId");

                    b.HasOne("Cpb.Database.Entities.DbUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("CoffeeRecipe");

                    b.Navigation("Machine");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeMachine", b =>
                {
                    b.Navigation("Links");

                    b.Navigation("Orders");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbCoffeeRecipe", b =>
                {
                    b.Navigation("Links");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbIngredient", b =>
                {
                    b.Navigation("Links");
                });

            modelBuilder.Entity("Cpb.Database.Entities.DbOrder", b =>
                {
                    b.Navigation("LockedIngredients");
                });
#pragma warning restore 612, 618
        }
    }
}
