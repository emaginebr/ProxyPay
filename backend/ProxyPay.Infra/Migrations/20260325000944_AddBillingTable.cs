using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace ProxyPay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "proxypay_billings",
                columns: table => new
                {
                    billing_id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    store_id = table.Column<long>(type: "bigint", nullable: true),
                    customer_id = table.Column<long>(type: "bigint", nullable: true),
                    frequency = table.Column<int>(type: "integer", nullable: false),
                    billing_strategy = table.Column<int>(type: "integer", nullable: false),
                    billing_start_date = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    created_at = table.Column<DateTime>(type: "timestamp without time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("proxypay_billings_pkey", x => x.billing_id);
                    table.ForeignKey(
                        name: "fk_proxypay_billing_customer",
                        column: x => x.customer_id,
                        principalTable: "proxypay_customers",
                        principalColumn: "customer_id");
                    table.ForeignKey(
                        name: "fk_proxypay_billing_store",
                        column: x => x.store_id,
                        principalTable: "proxypay_stores",
                        principalColumn: "store_id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_billings_customer_id",
                table: "proxypay_billings",
                column: "customer_id");

            migrationBuilder.CreateIndex(
                name: "IX_proxypay_billings_store_id",
                table: "proxypay_billings",
                column: "store_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "proxypay_billings");
        }
    }
}
