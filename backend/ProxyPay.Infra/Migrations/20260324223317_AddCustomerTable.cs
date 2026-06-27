using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProxyPay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proxypay_customers",
                columns: table => new
                {
                    customer_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    document_id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    cellphone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    email = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proxypay_customers_pkey", x => x.customer_id);
                });

            migrationBuilder.CreateTable(
                name: "proxypay_invoices",
                columns: table => new
                {
                    invoice_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    invoice_number = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    notes = table.Column<string>(type: "text", nullable: true),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    sub_total = table.Column<double>(type: "double precision", nullable: false),
                    discount = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    tax = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    total = table.Column<double>(type: "double precision", nullable: false),
                    due_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    paid_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proxypay_invoices_pkey", x => x.invoice_id);
                    table.ForeignKey(
                        name: "fk_proxypay_invoice_customer",
                        column: x => x.customer_id,
                        principalTable: "proxypay_customers",
                        principalColumn: "customer_id");
                });

            migrationBuilder.CreateTable(
                name: "proxypay_invoice_items",
                columns: table => new
                {
                    invoice_item_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    invoice_id = table.Column<long>(type: "bigint", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    quantity = table.Column<int>(type: "integer", nullable: false),
                    unit_price = table.Column<double>(type: "double precision", nullable: false),
                    discount = table.Column<double>(type: "double precision", nullable: false, defaultValue: 0.0),
                    total = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proxypay_invoice_items_pkey", x => x.invoice_item_id);
                    table.ForeignKey(
                        name: "fk_proxypay_invoice_item_invoice",
                        column: x => x.invoice_id,
                        principalTable: "proxypay_invoices",
                        principalColumn: "invoice_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "proxypay_transactions",
                columns: table => new
                {
                    transaction_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    invoice_id = table.Column<long>(type: "bigint", nullable: true),
                    type = table.Column<int>(type: "integer", nullable: false),
                    category = table.Column<int>(type: "integer", nullable: false),
                    description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    amount = table.Column<double>(type: "double precision", nullable: false),
                    balance = table.Column<double>(type: "double precision", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proxypay_transactions_pkey", x => x.transaction_id);
                    table.ForeignKey(
                        name: "fk_proxypay_transaction_invoice",
                        column: x => x.invoice_id,
                        principalTable: "proxypay_invoices",
                        principalColumn: "invoice_id");
                });

            migrationBuilder.CreateIndex(
                name: "ix_proxypay_customers_user_id",
                table: "proxypay_customers",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_invoice_items_invoice_id",
                table: "proxypay_invoice_items",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_invoices_customer_id",
                table: "proxypay_invoices",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "ix_proxypay_invoices_number",
                table: "proxypay_invoices",
                column: "invoice_number",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_transactions_invoice_id",
                table: "proxypay_transactions",
                column: "invoice_id");

            migrationBuilder.CreateIndex(
                name: "ix_proxypay_transactions_user_id",
                table: "proxypay_transactions",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proxypay_invoice_items");

            migrationBuilder.DropTable(
                name: "proxypay_transactions");

            migrationBuilder.DropTable(
                name: "proxypay_invoices");

            migrationBuilder.DropTable(
                name: "proxypay_customers");
        }
    }
}
