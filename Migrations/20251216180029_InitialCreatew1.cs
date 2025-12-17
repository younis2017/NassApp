using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nass.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatew1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agency",
                columns: table => new
                {
                    Agency_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Agency_name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    Agency_phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Agency_email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Agency_website = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Agency_address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Agency_location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Agency_tax_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Agency_tenet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Agency_username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Agency_password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Agency_joined_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Agency_logo = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Agency_uid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Agency__754F0F7CF043303F", x => x.Agency_id);
                });

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Categori__19093A0B80DCF49B", x => x.CategoryId);
                });

            migrationBuilder.CreateTable(
                name: "Customer",
                columns: table => new
                {
                    customer_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    customer_name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    customer_phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    customer_email = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    customer_address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    customer_location = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    customer_tax_id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    customer_tenet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    customer_username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    customer_password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    customer_joined_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Customer_uid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Customer__CD65CB85E53A9F05", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Trans_id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Trans_uid = table.Column<Guid>(type: "uniqueidentifier", nullable: false, defaultValueSql: "(newid())"),
                    Customer_id = table.Column<int>(type: "int", nullable: false),
                    Trans_date = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "(sysutcdatetime())"),
                    Trans_blob_attachmenet = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Trans_url_attachment = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Trans_categories = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Trans_description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Agency_id = table.Column<int>(type: "int", nullable: true),
                    trans_recived_date = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Trans_max_agency = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    Agency_tenat = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Trans_status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "PENDING")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK__Transact__7B18403588279D35", x => x.Trans_id);
                    table.ForeignKey(
                        name: "FK_Transactions_Agency",
                        column: x => x.Agency_id,
                        principalTable: "Agency",
                        principalColumn: "Agency_id");
                    table.ForeignKey(
                        name: "FK_Transactions_Customer",
                        column: x => x.Customer_id,
                        principalTable: "Customer",
                        principalColumn: "customer_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Agency_id",
                table: "Transactions",
                column: "Agency_id");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_Customer_id",
                table: "Transactions",
                column: "Customer_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "Agency");

            migrationBuilder.DropTable(
                name: "Customer");
        }
    }
}
