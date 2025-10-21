using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace FurnitureShop.Models
{
    // ==== 1. User Account ====
    public class ApplicationUser : IdentityUser
    {
        [Required, StringLength(100)]
        public string FullName { get; set; } = "";

        [StringLength(200)]
        public string? Address { get; set; }

        [StringLength(20)]
        public string? Gender { get; set; }
    }

    // ==== 2. Category ====
    public class Category
    {
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; } = "";

        [StringLength(300)]
        public string? Description { get; set; }

        public ICollection<Product>? Products { get; set; }
    }

    // ==== 3. Product ====
    public class Product
    {
        public int Id { get; set; }

        [Required, StringLength(150)]
        public string Name { get; set; } = "";

        [Range(0, double.MaxValue)]
        public decimal Price { get; set; }

        [StringLength(200)]
        public string Brand { get; set; } = "";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(255)]
        public string? ImageUrl { get; set; }

        public int CategoryId { get; set; }
        public Category? Category { get; set; }
    }

    // ==== 4. Cart ====
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        public ICollection<CartItem>? Items { get; set; }
    }

    public class CartItem
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }

        public int CartId { get; set; }
        public Cart? Cart { get; set; }
    }

    // ==== 5. Order ====
    public enum OrderStatus { Pending, Processing, Shipped, Completed, Cancelled }

    public class Order
    {
        public int Id { get; set; }
        public string UserId { get; set; } = "";
        public ApplicationUser? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;
        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public ICollection<OrderItem>? Items { get; set; }
    }

    public class OrderItem
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }

        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public int OrderId { get; set; }
        public Order? Order { get; set; }
    }
}
