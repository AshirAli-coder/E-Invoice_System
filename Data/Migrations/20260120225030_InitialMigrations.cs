using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Invoice_system.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigrations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "customers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_customers", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "invoices",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    invoice_no = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    seller_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seller_contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    seller_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customer_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customer_address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    customer_contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    prod_name_service = table.Column<string>(name: "prod_name/service", type: "nvarchar(max)", nullable: true),
                    qty_unit_type = table.Column<string>(name: "qty/unit_type", type: "nvarchar(max)", nullable: true),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    payment = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_invoices", x => x.id);
                });

        

            migrationBuilder.CreateTable(
                name: "products_services",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    prod_name_service = table.Column<string>(name: "prod_name/service", type: "nvarchar(max)", nullable: true),
                    barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    qty_unit_type = table.Column<string>(name: "qty/unit_type", type: "nvarchar(max)", nullable: true),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    expiry_date = table.Column<decimal>(type: "datetime2", nullable: false),
                    description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    tax = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_products_services", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "sellers",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    contact = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    address = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sellers", x => x.id);
                });
            migrationBuilder.CreateTable(
               name: "sale_details",
               columns: table => new
               {
                   id = table.Column<int>(type: "int", nullable: false)
                       .Annotation("SqlServer:Identity", "1, 1"),
                   customer_name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   date = table.Column<DateTime>(type: "datetime2", nullable: false),
                   prod_name_service = table.Column<string>(name: "prod_name/service", type: "nvarchar(max)", nullable: true),
                   barcode = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   qty_unit_type = table.Column<string>(name: "qty/unit_type", type: "nvarchar(max)", nullable: true),
                   price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                   discount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                   expiry_date = table.Column<decimal>(type: "datetime2", nullable: false),
                   total_price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                   payment_method = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   description = table.Column<string>(type: "nvarchar(max)", nullable: true),
                   status = table.Column<string>(type: "nvarchar(max)", nullable: true)
               },
               constraints: table =>
               {
                   table.PrimaryKey("PK_sale_details", x => x.id);
               });

            migrationBuilder.CreateTable(
                name: "users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    status = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_users", x => x.id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "customers");

            migrationBuilder.DropTable(
               name: "sale_details");

            migrationBuilder.DropTable(
                name: "products_services");

            migrationBuilder.DropTable(
                name: "sellers");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "invoices");
        }
    }
}
