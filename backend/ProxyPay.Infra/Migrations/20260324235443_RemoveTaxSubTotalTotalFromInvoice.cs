using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProxyPay.Infra.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaxSubTotalTotalFromInvoice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "sub_total",
                table: "proxypay_invoices");

            migrationBuilder.DropColumn(
                name: "tax",
                table: "proxypay_invoices");

            migrationBuilder.DropColumn(
                name: "total",
                table: "proxypay_invoices");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "sub_total",
                table: "proxypay_invoices",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "tax",
                table: "proxypay_invoices",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "total",
                table: "proxypay_invoices",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}
