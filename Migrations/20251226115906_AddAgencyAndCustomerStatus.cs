using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Nass.Migrations
{
    /// <inheritdoc />
    public partial class AddAgencyAndCustomerStatus : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Only add the new columns
            migrationBuilder.AddColumn<int>(
                name: "AgencyStatus",
                table: "Agencies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CustomerStatus",
                table: "Customers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove columns if rolling back
            migrationBuilder.DropColumn(
                name: "AgencyStatus",
                table: "Agencies");

            migrationBuilder.DropColumn(
                name: "CustomerStatus",
                table: "Customers");
        }
    }
}
