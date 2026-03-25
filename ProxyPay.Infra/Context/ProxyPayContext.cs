using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ProxyPay.Infra.Context;

public partial class ProxyPayContext : DbContext
{
    public ProxyPayContext()
    {
    }

    public ProxyPayContext(DbContextOptions<ProxyPayContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<BillingItem> BillingItems { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.StoreId).HasName("proxypay_stores_pkey");

            entity.ToTable("proxypay_stores");

            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.ClientId)
                .IsRequired()
                .HasMaxLength(32)
                .HasColumnName("client_id");
            entity.HasIndex(e => e.ClientId)
                .IsUnique()
                .HasDatabaseName("ix_proxypay_stores_client_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(240)
                .HasColumnName("name");
            entity.Property(e => e.Email)
                .HasMaxLength(240)
                .HasColumnName("email");
            entity.Property(e => e.AbacatePayApiKey)
                .HasMaxLength(500)
                .HasColumnName("abacatepay_api_key");
            entity.Property(e => e.BillingStrategy)
                .HasDefaultValue(1)
                .HasColumnName("billing_strategy");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_proxypay_stores_user_id");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("proxypay_customers_pkey");

            entity.ToTable("proxypay_customers");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(240)
                .HasColumnName("name");
            entity.Property(e => e.DocumentId)
                .HasMaxLength(20)
                .HasColumnName("document_id");
            entity.Property(e => e.Cellphone)
                .HasMaxLength(20)
                .HasColumnName("cellphone");
            entity.Property(e => e.Email)
                .HasMaxLength(240)
                .HasColumnName("email");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Store).WithMany(p => p.Customers)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proxypay_customer_store");
        });

        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("proxypay_invoices_pkey");

            entity.ToTable("proxypay_invoices");

            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("invoice_number");
            entity.HasIndex(e => e.InvoiceNumber)
                .IsUnique()
                .HasDatabaseName("ix_proxypay_invoices_number");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.Discount)
                .HasDefaultValue(0.0)
                .HasColumnName("discount");
            entity.Property(e => e.DueDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("due_date");
            entity.Property(e => e.PaidAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("paid_at");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Customer).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proxypay_invoice_customer");

            entity.HasOne(d => d.Store).WithMany(p => p.Invoices)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proxypay_invoice_store");
        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceItemId).HasName("proxypay_invoice_items_pkey");

            entity.ToTable("proxypay_invoice_items");

            entity.Property(e => e.InvoiceItemId).HasColumnName("invoice_item_id");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            entity.Property(e => e.Discount)
                .HasDefaultValue(0.0)
                .HasColumnName("discount");
            entity.Property(e => e.Total).HasColumnName("total");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Invoice).WithMany(p => p.InvoiceItems)
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_proxypay_invoice_item_invoice");
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.BillingId).HasName("proxypay_billings_pkey");

            entity.ToTable("proxypay_billings");

            entity.Property(e => e.BillingId).HasColumnName("billing_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.Frequency).HasColumnName("frequency");
            entity.Property(e => e.BillingStartDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("billing_start_date");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Store).WithMany(p => p.Billings)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proxypay_billing_store");

            entity.HasOne(d => d.Customer).WithMany(p => p.Billings)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proxypay_billing_customer");
        });

        modelBuilder.Entity<BillingItem>(entity =>
        {
            entity.HasKey(e => e.BillingItemId).HasName("proxypay_billing_items_pkey");

            entity.ToTable("proxypay_billing_items");

            entity.Property(e => e.BillingItemId).HasColumnName("billing_item_id");
            entity.Property(e => e.BillingId).HasColumnName("billing_id");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice).HasColumnName("unit_price");
            entity.Property(e => e.Discount)
                .HasDefaultValue(0.0)
                .HasColumnName("discount");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Billing).WithMany(p => p.BillingItems)
                .HasForeignKey(d => d.BillingId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("fk_proxypay_billing_item_billing");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("proxypay_transactions_pkey");

            entity.ToTable("proxypay_transactions");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.StoreId).HasColumnName("store_id");
            entity.Property(e => e.Type).HasColumnName("type");
            entity.Property(e => e.Category).HasColumnName("category");
            entity.Property(e => e.Description)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.Amount).HasColumnName("amount");
            entity.Property(e => e.Balance).HasColumnName("balance");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");

            entity.HasOne(d => d.Invoice).WithMany()
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proxypay_transaction_invoice");

            entity.HasOne(d => d.Store).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.StoreId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_proxypay_transaction_store");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
