using deliveryDomain.Model;
using Microsoft.EntityFrameworkCore;

namespace deliveryInfrastructure;

public partial class DeliveryBdContext : DbContext
{
    public DeliveryBdContext()
    {
    }

    public DeliveryBdContext(DbContextOptions<DeliveryBdContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }
    public virtual DbSet<Client> Clients { get; set; }
    public virtual DbSet<Courier> Couriers { get; set; }
    public virtual DbSet<Good> Goods { get; set; }
    public virtual DbSet<Order> Orders { get; set; }
    public virtual DbSet<OrderGood> OrderGoods { get; set; }
    public virtual DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(255)
                .HasColumnName("category_name");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK_Table_1");

            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Address)
                .HasMaxLength(255)
                .HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .HasColumnName("phone_number");
        });

        modelBuilder.Entity<Courier>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(255)
                .HasColumnName("phone_number");
            entity.Property(e => e.VehicleType)
                .HasMaxLength(255)
                .HasColumnName("vehicle_type");
        });

        modelBuilder.Entity<Good>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.CategoryId).HasColumnName("category_id");
            entity.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("price");

            entity.HasOne(d => d.Category).WithMany(p => p.Goods)
                .HasForeignKey(d => d.CategoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Goods_Categories");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.CourierId).HasColumnName("courier_id");
            entity.Property(e => e.DeliveryAddress)
                .HasMaxLength(255)
                .HasColumnName("delivery_address");
            entity.Property(e => e.OrderDate)
                .HasColumnType("datetime")
                .HasColumnName("order_date");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("total_amount");

            entity.HasOne(d => d.Courier).WithMany(p => p.Orders)
                .HasForeignKey(d => d.CourierId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Courier");

            entity.HasOne(o => o.Client).WithMany(c => c.Orders)
               .HasForeignKey(o => o.ClientId)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK_Orders_Clients");
        });

        modelBuilder.Entity<OrderGood>(entity =>
        {
            entity.ToTable("Order_Goods");

            entity.HasKey(og => new { og.OrderId, og.GoodId });

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.GoodId).HasColumnName("good_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(og => og.Order).WithMany(o => o.OrderGoods)
                .HasForeignKey(og => og.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Goods_Orders");

            entity.HasOne(og => og.Good).WithMany(g => g.OrderGoods)
                .HasForeignKey(og => og.GoodId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Order_Goods_Goods");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.Property(e => e.Id)
                .ValueGeneratedOnAdd()
                .HasColumnName("id");
            entity.Property(e => e.Amount)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("amount");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentDate)
                .HasColumnType("datetime")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(255)
                .HasColumnName("payment_method");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Payments_Orders");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}