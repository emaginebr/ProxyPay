using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProxyPay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddStoreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "store_id",
                table: "proxypay_transactions",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "store_id",
                table: "proxypay_invoices",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "store_id",
                table: "proxypay_customers",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "proxypay_stores",
                columns: table => new
                {
                    store_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<long>(type: "bigint", nullable: false),
                    name = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: false),
                    email = table.Column<string>(type: "character varying(240)", maxLength: 240, nullable: true),
                    abacatepay_api_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proxypay_stores_pkey", x => x.store_id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_transactions_store_id",
                table: "proxypay_transactions",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_invoices_store_id",
                table: "proxypay_invoices",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_customers_store_id",
                table: "proxypay_customers",
                column: "store_id");

            migrationBuilder.CreateIndex(
                name: "ix_proxypay_stores_user_id",
                table: "proxypay_stores",
                column: "user_id");

            migrationBuilder.AddForeignKey(
                name: "fk_proxypay_customer_store",
                table: "proxypay_customers",
                column: "store_id",
                principalTable: "proxypay_stores",
                principalColumn: "store_id");

            migrationBuilder.AddForeignKey(
                name: "fk_proxypay_invoice_store",
                table: "proxypay_invoices",
                column: "store_id",
                principalTable: "proxypay_stores",
                principalColumn: "store_id");

            migrationBuilder.AddForeignKey(
                name: "fk_proxypay_transaction_store",
                table: "proxypay_transactions",
                column: "store_id",
                principalTable: "proxypay_stores",
                principalColumn: "store_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_proxypay_customer_store",
                table: "proxypay_customers");

            migrationBuilder.DropForeignKey(
                name: "fk_proxypay_invoice_store",
                table: "proxypay_invoices");

            migrationBuilder.DropForeignKey(
                name: "fk_proxypay_transaction_store",
                table: "proxypay_transactions");

            migrationBuilder.DropTable(
                name: "proxypay_stores");

            migrationBuilder.DropIndex(
                name: "IX_proxypay_transactions_store_id",
                table: "proxypay_transactions");

            migrationBuilder.DropIndex(
                name: "IX_proxypay_invoices_store_id",
                table: "proxypay_invoices");

            migrationBuilder.DropIndex(
                name: "IX_proxypay_customers_store_id",
                table: "proxypay_customers");

            migrationBuilder.DropColumn(
                name: "store_id",
                table: "proxypay_transactions");

            migrationBuilder.DropColumn(
                name: "store_id",
                table: "proxypay_invoices");

            migrationBuilder.DropColumn(
                name: "store_id",
                table: "proxypay_customers");
        }
    }
}
