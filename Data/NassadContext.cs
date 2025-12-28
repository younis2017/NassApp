using Microsoft.EntityFrameworkCore;
using Nass.Models;

namespace Nass.Data
{
    public class NassadContext : DbContext
    {
        public NassadContext(DbContextOptions<NassadContext> options) : base(options) { }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Agencies> Agencies { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationRecipient> NotificationRecipients { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ----------------------------
            // Customer
            // ----------------------------
            modelBuilder.Entity<Customer>()
                .HasKey(c => c.CustomerId);

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerUid)
                .IsRequired();

            modelBuilder.Entity<Customer>()
                .Property(c => c.CustomerJoinedDate)
                .HasDefaultValueSql("GETDATE()");

            // ----------------------------
            // Agency
            // ----------------------------
            modelBuilder.Entity<Agencies>()
                .HasKey(a => a.AgencyId);

            modelBuilder.Entity<Agencies>()
                .Property(a => a.AgencyUid)
                .IsRequired();

            modelBuilder.Entity<Agencies>()
                .Property(a => a.AgencyJoinedDate)
                .HasDefaultValueSql("GETDATE()");

            // ----------------------------
            // Transaction
            // ----------------------------
            modelBuilder.Entity<Transaction>()
                .HasKey(t => t.Trans_id);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TransStatus)
                .HasDefaultValue(0); // or TransactionStatus.New

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Customer)
                .WithMany(c => c.Transactions)
                .HasForeignKey(t => t.Customer_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Transaction>()
                .HasOne(t => t.Agency)
                .WithMany(a => a.Transactions)
                .HasForeignKey(t => t.Agency_id)
                .OnDelete(DeleteBehavior.Restrict);
            // ----------------------------
            // Notification
            // ----------------------------
            modelBuilder.Entity<Notification>()
                .HasKey(n => n.NotificationId);

            modelBuilder.Entity<Notification>()
                .Property(n => n.NotificationId)
                .ValueGeneratedOnAdd(); // EF knows DB will generate identity

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Transaction)
                .WithMany(t => t.NotificationList)
                .HasForeignKey(n => n.Trans_Id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Notification>()
                .HasMany(n => n.Recipients)
                .WithOne(nr => nr.Notification)
                .HasForeignKey(nr => nr.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);

            // ----------------------------
            // NotificationRecipient
            // ----------------------------
            modelBuilder.Entity<NotificationRecipient>()
                .HasKey(nr => nr.NotificationRecipientId);

            modelBuilder.Entity<NotificationRecipient>()
                .Property(nr => nr.NotificationRecipientId)
                .ValueGeneratedOnAdd(); // EF auto-generates identity

            modelBuilder.Entity<NotificationRecipient>()
                .HasOne(nr => nr.Transaction)
                .WithMany(t => t.Notifications)
                .HasForeignKey(nr => nr.Trans_id)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<NotificationRecipient>()
                .HasOne(nr => nr.Agency)
                .WithMany(a => a.NotificationRecipients)
                .HasForeignKey(nr => nr.AgencyId)
                .OnDelete(DeleteBehavior.Restrict);

        }

    }
}
