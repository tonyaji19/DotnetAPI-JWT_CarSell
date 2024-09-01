using Microsoft.EntityFrameworkCore;
using CarSell.Models;
using System;

namespace CarSell.DataContext
{
    public class CarDbContext : DbContext
    {
        public CarDbContext(DbContextOptions<CarDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Konfigurasi relasi antara Product dan Image
            modelBuilder.Entity<Image>()
                .HasOne(p => p.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(p => p.ProductId);
        }
    }
}
