using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DataContext.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategoryWordRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoriesWords_Categories_CategoryId",
                table: "CategoriesWords");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoriesWords_Words_WordId",
                table: "CategoriesWords");

            migrationBuilder.DropForeignKey(
                name: "FK_Words_Categories_CategoryId",
                table: "Words");

            migrationBuilder.DropIndex(
                name: "IX_Words_CategoryId",
                table: "Words");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoriesWords",
                table: "CategoriesWords");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Words");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "Words");

            migrationBuilder.RenameTable(
                name: "CategoriesWords",
                newName: "CategoryWords");

            migrationBuilder.RenameIndex(
                name: "IX_CategoriesWords_WordId",
                table: "CategoryWords",
                newName: "IX_CategoryWords_WordId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoriesWords_CategoryId",
                table: "CategoryWords",
                newName: "IX_CategoryWords_CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "CategoryWords",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoryWords",
                table: "CategoryWords",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryWords_Categories_CategoryId",
                table: "CategoryWords",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoryWords_Words_WordId",
                table: "CategoryWords",
                column: "WordId",
                principalTable: "Words",
                principalColumn: "WordId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CategoryWords_Categories_CategoryId",
                table: "CategoryWords");

            migrationBuilder.DropForeignKey(
                name: "FK_CategoryWords_Words_WordId",
                table: "CategoryWords");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CategoryWords",
                table: "CategoryWords");

            migrationBuilder.DropColumn(
                name: "Frequency",
                table: "CategoryWords");

            migrationBuilder.RenameTable(
                name: "CategoryWords",
                newName: "CategoriesWords");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryWords_WordId",
                table: "CategoriesWords",
                newName: "IX_CategoriesWords_WordId");

            migrationBuilder.RenameIndex(
                name: "IX_CategoryWords_CategoryId",
                table: "CategoriesWords",
                newName: "IX_CategoriesWords_CategoryId");

            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Words",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Frequency",
                table: "Words",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_CategoriesWords",
                table: "CategoriesWords",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_Words_CategoryId",
                table: "Words",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriesWords_Categories_CategoryId",
                table: "CategoriesWords",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CategoriesWords_Words_WordId",
                table: "CategoriesWords",
                column: "WordId",
                principalTable: "Words",
                principalColumn: "WordId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Words_Categories_CategoryId",
                table: "Words",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
