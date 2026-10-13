using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mealthy.Api.Shared.Infraestructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddMedia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "photo_url",
                table: "products");

            migrationBuilder.AddColumn<int>(
                name: "image_id",
                table: "stores",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "image_id",
                table: "recipes",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "image_id",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "images",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    key = table.Column<Guid>(type: "uuid", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    content_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    declared_size_bytes = table.Column<long>(type: "bigint", nullable: false),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    width = table.Column<int>(type: "integer", nullable: true),
                    height = table.Column<int>(type: "integer", nullable: true),
                    stored_size_bytes = table.Column<long>(type: "bigint", nullable: true),
                    failure_reason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_images", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "ix_stores_image_id",
                table: "stores",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_recipes_image_id",
                table: "recipes",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_products_image_id",
                table: "products",
                column: "image_id");

            migrationBuilder.CreateIndex(
                name: "ix_images_created_at",
                table: "images",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "ix_images_key",
                table: "images",
                column: "key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_images_owner_id",
                table: "images",
                column: "owner_id");

            migrationBuilder.AddForeignKey(
                name: "fk_products_image_image_id",
                table: "products",
                column: "image_id",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_recipes_images_image_id",
                table: "recipes",
                column: "image_id",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "fk_stores_image_image_id",
                table: "stores",
                column: "image_id",
                principalTable: "images",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_products_image_image_id",
                table: "products");

            migrationBuilder.DropForeignKey(
                name: "fk_recipes_images_image_id",
                table: "recipes");

            migrationBuilder.DropForeignKey(
                name: "fk_stores_image_image_id",
                table: "stores");

            migrationBuilder.DropTable(
                name: "images");

            migrationBuilder.DropIndex(
                name: "ix_stores_image_id",
                table: "stores");

            migrationBuilder.DropIndex(
                name: "ix_recipes_image_id",
                table: "recipes");

            migrationBuilder.DropIndex(
                name: "ix_products_image_id",
                table: "products");

            migrationBuilder.DropColumn(
                name: "image_id",
                table: "stores");

            migrationBuilder.DropColumn(
                name: "image_id",
                table: "recipes");

            migrationBuilder.DropColumn(
                name: "image_id",
                table: "products");

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "stores",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "recipes",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "photo_url",
                table: "products",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
