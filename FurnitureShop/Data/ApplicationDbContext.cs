using FurnitureShop.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FurnitureShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        // ==== DbSets ====
        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<Cart> Carts => Set<Cart>();
        public DbSet<CartItem> CartItems => Set<CartItem>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        

        protected override void OnModelCreating(ModelBuilder b)
        {
            base.OnModelCreating(b);

            // ==== Quan hệ giữa các bảng ====

            // Category - Product (1-n)
            b.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

            // Cart - User (1-n)
            b.Entity<Cart>()
             .HasOne(c => c.User)
             .WithMany()
             .HasForeignKey(c => c.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // CartItem - Cart & Product (1-n)
            b.Entity<CartItem>()
             .HasOne(i => i.Cart)
             .WithMany(c => c.Items)
             .HasForeignKey(i => i.CartId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Entity<CartItem>()
             .HasOne(i => i.Product)
             .WithMany()
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            // Order - User (1-n)
            b.Entity<Order>()
             .HasOne(o => o.User)
             .WithMany()
             .HasForeignKey(o => o.UserId)
             .OnDelete(DeleteBehavior.Cascade);

            // OrderItem - Order & Product (1-n)
            b.Entity<OrderItem>()
             .HasOne(i => i.Order)
             .WithMany(o => o.Items)
             .HasForeignKey(i => i.OrderId)
             .OnDelete(DeleteBehavior.Cascade);

            b.Entity<OrderItem>()
             .HasOne(i => i.Product)
             .WithMany()
             .HasForeignKey(i => i.ProductId)
             .OnDelete(DeleteBehavior.Restrict);

            // ==== Cấu hình kiểu dữ liệu cho decimal ====

            b.Entity<Product>()
             .Property(p => p.Price)
             .HasColumnType("decimal(18,2)");

            b.Entity<OrderItem>()
             .Property(i => i.Price)
             .HasColumnType("decimal(18,2)");

            b.Entity<Order>()
             .Property(o => o.TotalAmount)
             .HasColumnType("decimal(18,2)");
        }
    }
}
