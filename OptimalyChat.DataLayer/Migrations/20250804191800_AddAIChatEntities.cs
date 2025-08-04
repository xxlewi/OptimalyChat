using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace OptimalyChat.DataLayer.Migrations
{
    /// <inheritdoc />
    public partial class AddAIChatEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TemplateProducts");

            migrationBuilder.DropTable(
                name: "TemplateCategories");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(128)",
                oldMaxLength: 128);

            migrationBuilder.CreateTable(
                name: "AIModels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModelId = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Provider = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ApiKey = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MaxTokens = table.Column<int>(type: "integer", nullable: false, defaultValue: 4096),
                    Temperature = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.69999999999999996),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CostPer1KInput = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    CostPer1KOutput = table.Column<decimal>(type: "numeric(10,6)", precision: 10, scale: 6, nullable: true),
                    Capabilities = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIModels", x => x.Id);
                    table.CheckConstraint("CK_AIModels_MaxTokens", "\"MaxTokens\" > 0");
                    table.CheckConstraint("CK_AIModels_Temperature", "\"Temperature\" >= 0 AND \"Temperature\" <= 2");
                });

            migrationBuilder.CreateTable(
                name: "Projects",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsEncrypted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EncryptionLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    EncryptionKeyId = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Projects", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Projects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Conversations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    LastMessageAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    TotalTokensUsed = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conversations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Conversations_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Documents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ProjectId = table.Column<int>(type: "integer", nullable: false),
                    FileName = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    StoragePath = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    ExtractedText = table.Column<string>(type: "text", nullable: true),
                    IsIndexed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ChunkCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Documents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Documents_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConversationId = table.Column<int>(type: "integer", nullable: false),
                    Role = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "text", nullable: true),
                    EncryptedContent = table.Column<string>(type: "text", nullable: true),
                    Nonce = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Embedding = table.Column<byte[]>(type: "bytea", nullable: true),
                    IsIndexed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    TokenCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Model = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ResponseTimeMs = table.Column<int>(type: "integer", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Messages_Conversations_ConversationId",
                        column: x => x.ConversationId,
                        principalTable: "Conversations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AIModels",
                columns: new[] { "Id", "ApiKey", "Capabilities", "CostPer1KInput", "CostPer1KOutput", "CreatedAt", "CreatedBy", "DeletedAt", "DeletedBy", "Endpoint", "IsActive", "IsDefault", "IsDeleted", "MaxTokens", "ModelId", "Name", "Provider", "Temperature", "UpdatedAt", "UpdatedBy" },
                values: new object[] { 1, null, null, null, null, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), null, null, null, "http://localhost:1234/v1", true, true, false, 4096, "local-model", "Local LM Studio Model", "LMStudio", 0.69999999999999996, null, null });

            migrationBuilder.CreateIndex(
                name: "IX_AIModels_IsActive",
                table: "AIModels",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AIModels_IsDefault",
                table: "AIModels",
                column: "IsDefault");

            migrationBuilder.CreateIndex(
                name: "IX_AIModels_ModelId",
                table: "AIModels",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_LastMessageAt",
                table: "Conversations",
                column: "LastMessageAt");

            migrationBuilder.CreateIndex(
                name: "IX_Conversations_ProjectId",
                table: "Conversations",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ContentType",
                table: "Documents",
                column: "ContentType");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_IsIndexed",
                table: "Documents",
                column: "IsIndexed");

            migrationBuilder.CreateIndex(
                name: "IX_Documents_ProjectId",
                table: "Documents",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId",
                table: "Messages",
                column: "ConversationId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ConversationId_CreatedAt",
                table: "Messages",
                columns: new[] { "ConversationId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_IsIndexed",
                table: "Messages",
                column: "IsIndexed");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_Role",
                table: "Messages",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_Name",
                table: "Projects",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Projects_UserId",
                table: "Projects",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIModels");

            migrationBuilder.DropTable(
                name: "Documents");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Conversations");

            migrationBuilder.DropTable(
                name: "Projects");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserTokens",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "ProviderKey",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LoginProvider",
                table: "AspNetUserLogins",
                type: "character varying(128)",
                maxLength: 128,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "TemplateCategories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DisplayOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateCategories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TemplateProducts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CategoryId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    IsFeatured = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Price = table.Column<decimal>(type: "numeric(18,2)", nullable: false),
                    SalePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Sku = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StockQuantity = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TemplateProducts", x => x.Id);
                    table.CheckConstraint("CK_TemplateProduct_Price_Positive", "\"Price\" > 0");
                    table.CheckConstraint("CK_TemplateProduct_SalePrice_LessThanPrice", "\"SalePrice\" IS NULL OR \"SalePrice\" < \"Price\"");
                    table.CheckConstraint("CK_TemplateProduct_SalePrice_Positive", "\"SalePrice\" IS NULL OR \"SalePrice\" > 0");
                    table.CheckConstraint("CK_TemplateProduct_StockQuantity_NonNegative", "\"StockQuantity\" >= 0");
                    table.ForeignKey(
                        name: "FK_TemplateProducts_TemplateCategories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "TemplateCategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "TemplateCategories",
                columns: new[] { "Id", "CreatedAt", "Description", "DisplayOrder", "IsActive", "IsDeleted", "Name", "UpdatedAt" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Elektronické zařízení", 1, true, false, "Elektronika", null },
                    { 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Módní oblečení", 2, true, false, "Oblečení", null },
                    { 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Knihy a publikace", 3, true, false, "Knihy", null }
                });

            migrationBuilder.InsertData(
                table: "TemplateProducts",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsActive", "IsDeleted", "IsFeatured", "Name", "Price", "SalePrice", "Sku", "StockQuantity", "UpdatedAt" },
                values: new object[] { 1, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Nejnovější iPhone s titanovým designem", true, false, true, "iPhone 15 Pro", 32990m, 29990m, "IPH15PRO", 15, null });

            migrationBuilder.InsertData(
                table: "TemplateProducts",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsActive", "IsDeleted", "Name", "Price", "SalePrice", "Sku", "StockQuantity", "UpdatedAt" },
                values: new object[,]
                {
                    { 2, 1, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Pokročilý Android smartphone", true, false, "Samsung Galaxy S24", 24990m, null, "SGS24", 8, null },
                    { 3, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Elegantní bavlněná košile", true, false, "Pánská košile", 1290m, null, "SHIRT001", 25, null }
                });

            migrationBuilder.InsertData(
                table: "TemplateProducts",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsActive", "IsDeleted", "Name", "Price", "SalePrice", "Sku", "UpdatedAt" },
                values: new object[] { 4, 3, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Kniha o psaní kvalitního kódu", true, false, "Čistý kód", 590m, null, "BOOK001", null });

            migrationBuilder.InsertData(
                table: "TemplateProducts",
                columns: new[] { "Id", "CategoryId", "CreatedAt", "Description", "IsActive", "IsDeleted", "IsFeatured", "Name", "Price", "SalePrice", "Sku", "StockQuantity", "UpdatedAt" },
                values: new object[] { 5, 2, new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Letní šaty v moderním stylu", true, false, true, "Dámské šaty", 2490m, 1990m, "DRESS001", 3, null });

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCategories_DisplayOrder",
                table: "TemplateCategories",
                column: "DisplayOrder");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateCategories_Name",
                table: "TemplateCategories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TemplateProducts_CategoryId",
                table: "TemplateProducts",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateProducts_IsActive",
                table: "TemplateProducts",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateProducts_IsFeatured",
                table: "TemplateProducts",
                column: "IsFeatured");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateProducts_Name",
                table: "TemplateProducts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_TemplateProducts_Sku",
                table: "TemplateProducts",
                column: "Sku",
                unique: true,
                filter: "\"Sku\" IS NOT NULL");
        }
    }
}
