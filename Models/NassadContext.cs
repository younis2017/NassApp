using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Nass.Models;

public partial class NassadContext : DbContext
{
    public NassadContext()
    {
    }

    public NassadContext(DbContextOptions<NassadContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Agency> Agencies { get; set; }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=DESKTOP-AVG95LF\\humber;Database=nassad;Trusted_Connection=True;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Agency>(entity =>
        {
            entity.HasKey(e => e.AgencyId).HasName("PK__Agency__754F0F7CF043303F");

            entity.ToTable("Agency");

            entity.Property(e => e.AgencyId).HasColumnName("Agency_id");
            entity.Property(e => e.AgencyAddress)
                .HasMaxLength(500)
                .HasColumnName("Agency_address");
            entity.Property(e => e.AgencyEmail)
                .HasMaxLength(200)
                .HasColumnName("Agency_email");
            entity.Property(e => e.AgencyJoinedDate)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("Agency_joined_date");
            entity.Property(e => e.AgencyLocation)
                .HasMaxLength(200)
                .HasColumnName("Agency_location");
            entity.Property(e => e.AgencyLogo)
                .HasMaxLength(255)
                .HasColumnName("Agency_logo");
            entity.Property(e => e.AgencyName)
                .HasMaxLength(250)
                .HasColumnName("Agency_name");
            entity.Property(e => e.AgencyPassword)
                .HasMaxLength(100)
                .HasColumnName("Agency_password");
            entity.Property(e => e.AgencyPhone)
                .HasMaxLength(50)
                .HasColumnName("Agency_phone");
            entity.Property(e => e.AgencyTaxId)
                .HasMaxLength(100)
                .HasColumnName("Agency_tax_id");
            entity.Property(e => e.AgencyTenet)
                .HasMaxLength(50)
                .HasColumnName("Agency_tenet");
            entity.Property(e => e.AgencyUid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("Agency_uid");
            entity.Property(e => e.AgencyUsername)
                .HasMaxLength(100)
                .HasColumnName("Agency_username");
            entity.Property(e => e.AgencyWebsite)
                .HasMaxLength(250)
                .HasColumnName("Agency_website");
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.CategoryId).HasName("PK__Categori__19093A0B80DCF49B");

            entity.Property(e => e.CreatedDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.Name).HasMaxLength(150);
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PK__Customer__CD65CB85E53A9F05");

            entity.ToTable("Customer");

            entity.Property(e => e.CustomerId).HasColumnName("customer_id");
            entity.Property(e => e.CustomerAddress)
                .HasMaxLength(500)
                .HasColumnName("customer_address");
            entity.Property(e => e.CustomerEmail)
                .HasMaxLength(200)
                .HasColumnName("customer_email");
            entity.Property(e => e.CustomerJoinedDate)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("customer_joined_date");
            entity.Property(e => e.CustomerLocation)
                .HasMaxLength(200)
                .HasColumnName("customer_location");
            entity.Property(e => e.CustomerName)
                .HasMaxLength(200)
                .HasColumnName("customer_name");
            entity.Property(e => e.CustomerPassword)
                .HasMaxLength(100)
                .HasColumnName("customer_password");
            entity.Property(e => e.CustomerPhone)
                .HasMaxLength(50)
                .HasColumnName("customer_phone");
            entity.Property(e => e.CustomerTaxId)
                .HasMaxLength(100)
                .HasColumnName("customer_tax_id");
            entity.Property(e => e.CustomerTenet)
                .HasMaxLength(50)
                .HasColumnName("customer_tenet");
            entity.Property(e => e.CustomerUid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("Customer_uid");
            entity.Property(e => e.CustomerUsername)
                .HasMaxLength(100)
                .HasColumnName("customer_username");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransId).HasName("PK__Transact__7B18403588279D35");

            entity.Property(e => e.TransId).HasColumnName("Trans_id");
            entity.Property(e => e.AgencyId).HasColumnName("Agency_id");
            entity.Property(e => e.AgencyTenat)
                .HasMaxLength(50)
                .HasColumnName("Agency_tenat");
            entity.Property(e => e.CustomerId).HasColumnName("Customer_id");
            entity.Property(e => e.TransBlobAttachmenet).HasColumnName("Trans_blob_attachmenet");
            entity.Property(e => e.TransCategories)
                .HasMaxLength(100)
                .HasColumnName("Trans_categories");
            entity.Property(e => e.TransDate)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnName("Trans_date");
            entity.Property(e => e.TransDescription).HasColumnName("Trans_description");
            entity.Property(e => e.TransMaxAgency)
                .HasDefaultValue(1)
                .HasColumnName("Trans_max_agency");
            entity.Property(e => e.TransRecivedDate).HasColumnName("trans_recived_date");
            entity.Property(e => e.TransStatus)
                .HasMaxLength(50)
                .HasDefaultValue("PENDING")
                .HasColumnName("Trans_status");
            entity.Property(e => e.TransUid)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("Trans_uid");
            entity.Property(e => e.TransUrlAttachment)
                .HasMaxLength(2000)
                .HasColumnName("Trans_url_attachment");

            entity.HasOne(d => d.Agency).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.AgencyId)
                .HasConstraintName("FK_Transactions_Agency");

            entity.HasOne(d => d.Customer).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.CustomerId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Transactions_Customer");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
