using Microsoft.EntityFrameworkCore;
using Doanmh.Model;

namespace Doanmh.Model
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet cho các bảng
        public DbSet<User> Users { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Cấu hình Order
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);

                entity.HasOne(o => o.User)
                      .WithMany()
                      .HasForeignKey(o => o.UserId)
                      .OnDelete(DeleteBehavior.Restrict)
                      .HasConstraintName("FK_Orders_Users_UserId");

                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired()
                      .HasDefaultValue(0m);

                entity.Property(o => o.Address)
                      .HasMaxLength(500)
                      .IsRequired()
       
                      .HasDefaultValue(string.Empty);

                entity.Property(o => o.Note)
                      .HasMaxLength(1000)
                      .HasDefaultValue(string.Empty);

                entity.Property(o => o.OrderDate)
                      .HasDefaultValueSql("GETDATE()");

                entity.HasMany(o => o.Items)
                      .WithOne()
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // Cấu hình Product
            modelBuilder.Entity<Product>(entity =>
            {
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)")
                      .IsRequired()
                      .HasDefaultValue(0m);
            });

            // Cấu hình OrderItem
            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.HasOne(oi => oi.Order).WithMany(o => o.Items).HasForeignKey(oi => oi.OrderId); // Sử dụng OrderId
                entity.HasOne(oi => oi.Product).WithMany().HasForeignKey(oi => oi.ProductId);
            });

            // Cấu hình khác (nếu có)
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("your_connection_string")
                              .LogTo(Console.WriteLine, LogLevel.Information)
                              .EnableSensitiveDataLogging(); // Hiển thị giá trị tham số
            }
        }
    }
}
