using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Ganesha.Infra.Context;

public partial class GaneshaContext : DbContext
{
    public GaneshaContext()
    {
    }

    public GaneshaContext(DbContextOptions<GaneshaContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Invoice> Invoices { get; set; }

    public virtual DbSet<InvoiceItem> InvoiceItems { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Invoice>(entity =>
        {
            entity.HasKey(e => e.InvoiceId).HasName("ganesha_invoices_pkey");

            entity.ToTable("ganesha_invoices");

            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.InvoiceNumber)
                .IsRequired()
                .HasMaxLength(50)
                .HasColumnName("invoice_number");
            entity.HasIndex(e => e.InvoiceNumber)
                .IsUnique()
                .HasDatabaseName("ix_ganesha_invoices_number");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Status)
                .HasDefaultValue(1)
                .HasColumnName("status");
            entity.Property(e => e.SubTotal).HasColumnName("sub_total");
            entity.Property(e => e.Discount)
                .HasDefaultValue(0.0)
                .HasColumnName("discount");
            entity.Property(e => e.Tax)
                .HasDefaultValue(0.0)
                .HasColumnName("tax");
            entity.Property(e => e.Total).HasColumnName("total");
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

        });

        modelBuilder.Entity<InvoiceItem>(entity =>
        {
            entity.HasKey(e => e.InvoiceItemId).HasName("ganesha_invoice_items_pkey");

            entity.ToTable("ganesha_invoice_items");

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
                .HasConstraintName("fk_ganesha_invoice_item_invoice");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("ganesha_transactions_pkey");

            entity.ToTable("ganesha_transactions");

            entity.Property(e => e.TransactionId).HasColumnName("transaction_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.InvoiceId).HasColumnName("invoice_id");
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

            entity.HasIndex(e => e.UserId)
                .HasDatabaseName("ix_ganesha_transactions_user_id");

            entity.HasOne(d => d.Invoice).WithMany()
                .HasForeignKey(d => d.InvoiceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_ganesha_transaction_invoice");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
