using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace AutoPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Brands",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LogoUrl = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Brands", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataSources",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Country = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Language = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataSources", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Markets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Markets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AvatarUrl = table.Column<string>(type: "text", nullable: true),
                    TelegramId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Models",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Category = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Models", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Models_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dealers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ContactInfo = table.Column<string>(type: "text", nullable: true),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Rating = table.Column<decimal>(type: "numeric", nullable: false, defaultValue: 0m),
                    MarketId = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dealers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dealers_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CarSearchQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: true),
                    ModelId = table.Column<int>(type: "integer", nullable: true),
                    Generation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    YearFrom = table.Column<int>(type: "integer", nullable: false),
                    YearTo = table.Column<int>(type: "integer", nullable: false),
                    Regions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Priority = table.Column<int>(type: "integer", nullable: false),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastParsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextParseAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarSearchQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarSearchQueues_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CarSearchQueues_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BrandId = table.Column<int>(type: "integer", nullable: true),
                    ModelId = table.Column<int>(type: "integer", nullable: true),
                    Generation = table.Column<string>(type: "text", nullable: true),
                    BodyTypes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Engines = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Transmission = table.Column<string>(type: "text", nullable: true),
                    YearFrom = table.Column<int>(type: "integer", nullable: true),
                    YearTo = table.Column<int>(type: "integer", nullable: true),
                    MaxMileage = table.Column<int>(type: "integer", nullable: true),
                    BudgetFrom = table.Column<decimal>(type: "numeric", nullable: true),
                    BudgetTo = table.Column<decimal>(type: "numeric", nullable: true),
                    Regions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    NotifyOnNew = table.Column<bool>(type: "boolean", nullable: false),
                    NotifyOnPriceDrop = table.Column<bool>(type: "boolean", nullable: false),
                    PriceDropThreshold = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPreferences_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserPreferences_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSearchs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    BrandId = table.Column<int>(type: "integer", nullable: true),
                    ModelId = table.Column<int>(type: "integer", nullable: true),
                    Generation = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    YearFrom = table.Column<int>(type: "integer", nullable: true),
                    YearTo = table.Column<int>(type: "integer", nullable: true),
                    MaxPrice = table.Column<decimal>(type: "numeric", nullable: true),
                    MaxMileage = table.Column<int>(type: "integer", nullable: true),
                    Regions = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastNotifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSearchs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSearchs_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserSearchs_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UserSearchs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Cars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    BrandId = table.Column<int>(type: "integer", nullable: false),
                    ModelId = table.Column<int>(type: "integer", nullable: false),
                    MarketId = table.Column<int>(type: "integer", nullable: false),
                    DealerId = table.Column<int>(type: "integer", nullable: true),
                    DataSourceId = table.Column<int>(type: "integer", nullable: true),
                    Year = table.Column<int>(type: "integer", nullable: false),
                    Price = table.Column<decimal>(type: "numeric", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Vin = table.Column<string>(type: "character varying(17)", maxLength: 17, nullable: true),
                    Mileage = table.Column<int>(type: "integer", nullable: false),
                    Transmission = table.Column<string>(type: "text", nullable: true),
                    Engine = table.Column<string>(type: "text", nullable: true),
                    FuelType = table.Column<string>(type: "text", nullable: true),
                    Color = table.Column<string>(type: "text", nullable: true),
                    Location = table.Column<string>(type: "text", nullable: true),
                    Country = table.Column<string>(type: "text", nullable: true),
                    SourceUrl = table.Column<string>(type: "text", nullable: false),
                    ImageUrl = table.Column<string>(type: "text", nullable: true),
                    IsAvailable = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SoldAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cars_Brands_BrandId",
                        column: x => x.BrandId,
                        principalTable: "Brands",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cars_DataSources_DataSourceId",
                        column: x => x.DataSourceId,
                        principalTable: "DataSources",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cars_Dealers_DealerId",
                        column: x => x.DealerId,
                        principalTable: "Dealers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Cars_Markets_MarketId",
                        column: x => x.MarketId,
                        principalTable: "Markets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Cars_Models_ModelId",
                        column: x => x.ModelId,
                        principalTable: "Models",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserSearchQueues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserSearchId = table.Column<int>(type: "integer", nullable: false),
                    CarSearchQueueId = table.Column<int>(type: "integer", nullable: false),
                    IsNewCarsNotified = table.Column<bool>(type: "boolean", nullable: false),
                    IsPriceDropNotified = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSearchQueues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSearchQueues_CarSearchQueues_CarSearchQueueId",
                        column: x => x.CarSearchQueueId,
                        principalTable: "CarSearchQueues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserSearchQueues_UserSearchs_UserSearchId",
                        column: x => x.UserSearchId,
                        principalTable: "UserSearchs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CarAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CarId = table.Column<int>(type: "integer", nullable: true),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReadAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarAlerts_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CarAlerts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PriceHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CarId = table.Column<int>(type: "integer", nullable: false),
                    OldPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    OldCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    NewPrice = table.Column<decimal>(type: "numeric", nullable: false),
                    NewCurrency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PriceHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PriceHistories_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCars",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    CarId = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsFavorite = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCars", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCars_Cars_CarId",
                        column: x => x.CarId,
                        principalTable: "Cars",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCars_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Brands_Name",
                table: "Brands",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CarAlerts_CarId",
                table: "CarAlerts",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CarAlerts_UserId",
                table: "CarAlerts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_BrandId",
                table: "Cars",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_DataSourceId",
                table: "Cars",
                column: "DataSourceId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_DealerId",
                table: "Cars",
                column: "DealerId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_MarketId",
                table: "Cars",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_ModelId",
                table: "Cars",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_Cars_SourceUrl",
                table: "Cars",
                column: "SourceUrl",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cars_Vin",
                table: "Cars",
                column: "Vin",
                filter: "\"Vin\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CarSearchQueues_BrandId_ModelId_Generation_YearFrom_YearTo",
                table: "CarSearchQueues",
                columns: new[] { "BrandId", "ModelId", "Generation", "YearFrom", "YearTo" });

            migrationBuilder.CreateIndex(
                name: "IX_CarSearchQueues_ModelId",
                table: "CarSearchQueues",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_DataSources_Name_Country",
                table: "DataSources",
                columns: new[] { "Name", "Country" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Dealers_MarketId",
                table: "Dealers",
                column: "MarketId");

            migrationBuilder.CreateIndex(
                name: "IX_Markets_Name",
                table: "Markets",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Models_BrandId_Name",
                table: "Models",
                columns: new[] { "BrandId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PriceHistories_CarId",
                table: "PriceHistories",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCars_CarId",
                table: "UserCars",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCars_UserId_CarId",
                table: "UserCars",
                columns: new[] { "UserId", "CarId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_BrandId",
                table: "UserPreferences",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_ModelId",
                table: "UserPreferences",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_UserId",
                table: "UserPreferences",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_TelegramId",
                table: "Users",
                column: "TelegramId",
                filter: "\"TelegramId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserSearchQueues_CarSearchQueueId",
                table: "UserSearchQueues",
                column: "CarSearchQueueId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSearchQueues_UserSearchId_CarSearchQueueId",
                table: "UserSearchQueues",
                columns: new[] { "UserSearchId", "CarSearchQueueId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSearchs_BrandId",
                table: "UserSearchs",
                column: "BrandId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSearchs_ModelId",
                table: "UserSearchs",
                column: "ModelId");

            migrationBuilder.CreateIndex(
                name: "IX_UserSearchs_UserId",
                table: "UserSearchs",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarAlerts");

            migrationBuilder.DropTable(
                name: "PriceHistories");

            migrationBuilder.DropTable(
                name: "UserCars");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "UserSearchQueues");

            migrationBuilder.DropTable(
                name: "Cars");

            migrationBuilder.DropTable(
                name: "CarSearchQueues");

            migrationBuilder.DropTable(
                name: "UserSearchs");

            migrationBuilder.DropTable(
                name: "DataSources");

            migrationBuilder.DropTable(
                name: "Dealers");

            migrationBuilder.DropTable(
                name: "Models");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Markets");

            migrationBuilder.DropTable(
                name: "Brands");
        }
    }
}
