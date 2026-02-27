// Data/ApplicationDbContext.cs (Ensure all DbSets are included)
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SamarStoneQwen.Models;

namespace SamarStoneQwen.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<PurchaseOrder> PurchaseOrders { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<Slab> Slabs { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Sale> Sales { get; set; }
        public DbSet<SaleItem> SaleItems { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }
        public DbSet<MarbleType> MarbleTypes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

           
            // Configure relationships
            builder.Entity<Container>()
                .HasOne(c => c.PurchaseOrder)
                .WithMany(po => po.Containers)
                .HasForeignKey(c => c.PurchaseOrderId);

            builder.Entity<Slab>()
                .HasOne(s => s.Container)
                .WithMany(c => c.Slabs)
                .HasForeignKey(s => s.ContainerId);

            builder.Entity<Slab>()
                .HasOne(s => s.MarbleTypeNavigation)
                .WithMany(mt => mt.Slabs)
                .HasForeignKey(s => s.MarbleTypeId);

            builder.Entity<Payment>()
                .HasOne(p => p.PurchaseOrder)
                .WithMany(po => po.Payments)
                .HasForeignKey(p => p.PurchaseOrderId);

            builder.Entity<SaleItem>()
                .HasOne(si => si.Sale)
                .WithMany(s => s.SaleItems)
                .HasForeignKey(si => si.SaleId);

            builder.Entity<SaleItem>()
                .HasOne(si => si.Slab)
                .WithMany()
                .HasForeignKey(si => si.SlabId);

            builder.Entity<Sale>()
                .HasOne(s => s.Customer)
                .WithMany(c => c.Sales)
                .HasForeignKey(s => s.CustomerId);

            builder.Entity<PurchaseOrder>()
                .HasOne(po => po.Supplier)
                .WithMany(s => s.PurchaseOrders)
                .HasForeignKey(po => po.SupplierId);
        }
    }
}