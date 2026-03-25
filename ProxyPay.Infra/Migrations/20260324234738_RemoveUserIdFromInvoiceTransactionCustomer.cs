using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProxyPay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUserIdFromInvoiceTransactionCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "ix_proxypay_transactions_user_id",
                table: "proxypay_transactions");

            migrationBuilder.DropIndex(
                name: "ix_proxypay_customers_user_id",
                table: "proxypay_customers");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "proxypay_transactions");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "proxypay_invoices");

            migrationBuilder.DropColumn(
                name: "user_id",
                table: "proxypay_customers");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "proxypay_transactions",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "proxypay_invoices",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<long>(
                name: "user_id",
                table: "proxypay_customers",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "ix_proxypay_transactions_user_id",
                table: "proxypay_transactions",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_proxypay_customers_user_id",
                table: "proxypay_customers",
                column: "user_id");
        }
    }
}
