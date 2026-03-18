using CafeManagement.Models.Domain;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CafeManagement.Data;

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // Master Data
    public DbSet<Store> Stores => Set<Store>();
    public DbSet<JobPosition> JobPositions => Set<JobPosition>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<MenuItem> MenuItems => Set<MenuItem>();
    public DbSet<Topping> Toppings => Set<Topping>();
    public DbSet<PaymentMethod> PaymentMethods => Set<PaymentMethod>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();

    // CRM
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<PointHistory> PointHistories => Set<PointHistory>();

    // POS
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderDetail> OrderDetails => Set<OrderDetail>();
    public DbSet<OrderDetailTopping> OrderDetailToppings => Set<OrderDetailTopping>();
    public DbSet<Transaction> Transactions => Set<Transaction>();

    // Inventory
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<InventoryStock> InventoryStocks => Set<InventoryStock>();
    public DbSet<InventoryLog> InventoryLogs => Set<InventoryLog>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderDetail> PurchaseOrderDetails => Set<PurchaseOrderDetail>();

    // HRM
    public DbSet<Shift> Shifts => Set<Shift>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<Timekeeping> Timekeepings => Set<Timekeeping>();
    public DbSet<ShiftHandover> ShiftHandovers => Set<ShiftHandover>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Customer: Unique phone
        builder.Entity<Customer>()
            .HasIndex(x => x.Phone)
            .IsUnique();

        // InventoryStock: Unique (StoreId, IngredientId)
        builder.Entity<InventoryStock>()
            .HasIndex(x => new { x.StoreId, x.IngredientId })
            .IsUnique();

        // Decimal precision
        builder.Entity<JobPosition>()
            .Property(x => x.HourlyRate).HasColumnType("decimal(12,2)");
        builder.Entity<MenuItem>()
            .Property(x => x.BasePrice).HasColumnType("decimal(12,0)");
        builder.Entity<Topping>()
            .Property(x => x.Price).HasColumnType("decimal(12,0)");
        builder.Entity<Order>()
            .Property(x => x.TotalAmount).HasColumnType("decimal(12,0)");
        builder.Entity<Order>()
            .Property(x => x.DiscountAmount).HasColumnType("decimal(12,0)");
        builder.Entity<Order>()
            .Property(x => x.FinalAmount).HasColumnType("decimal(12,0)");
        builder.Entity<OrderDetail>()
            .Property(x => x.UnitPrice).HasColumnType("decimal(12,0)");
        builder.Entity<OrderDetailTopping>()
            .Property(x => x.UnitPrice).HasColumnType("decimal(12,0)");
        builder.Entity<Transaction>()
            .Property(x => x.Amount).HasColumnType("decimal(12,0)");
        builder.Entity<Transaction>()
            .Property(x => x.AmountTendered).HasColumnType("decimal(12,0)");
        builder.Entity<Transaction>()
            .Property(x => x.ChangeAmount).HasColumnType("decimal(12,0)");
        builder.Entity<Recipe>()
            .Property(x => x.Quantity).HasColumnType("decimal(10,4)");
        builder.Entity<Recipe>()
            .Property(x => x.WastePercent).HasColumnType("decimal(5,2)");
        builder.Entity<Ingredient>()
            .Property(x => x.MinStockLevel).HasColumnType("decimal(10,2)");
        builder.Entity<InventoryStock>()
            .Property(x => x.CurrentQuantity).HasColumnType("decimal(10,4)");
        builder.Entity<InventoryLog>()
            .Property(x => x.ChangeQuantity).HasColumnType("decimal(10,4)");
        builder.Entity<PurchaseOrderDetail>()
            .Property(x => x.Quantity).HasColumnType("decimal(10,4)");
        builder.Entity<PurchaseOrderDetail>()
            .Property(x => x.CostPrice).HasColumnType("decimal(12,2)");
        builder.Entity<Timekeeping>()
            .Property(x => x.TotalHours).HasColumnType("decimal(5,2)");
        builder.Entity<Timekeeping>()
            .Property(x => x.HourlyRateAtTime).HasColumnType("decimal(12,2)");
        builder.Entity<ShiftHandover>()
            .Property(x => x.OpeningCash).HasColumnType("decimal(12,0)");
        builder.Entity<ShiftHandover>()
            .Property(x => x.ExpectedCash).HasColumnType("decimal(12,0)");
        builder.Entity<ShiftHandover>()
            .Property(x => x.ActualCashCounted).HasColumnType("decimal(12,0)");
        builder.Entity<ShiftHandover>()
            .Property(x => x.Difference).HasColumnType("decimal(12,0)");

        // AppUser FK relationships
        builder.Entity<AppUser>()
            .HasOne(u => u.Position)
            .WithMany(p => p.Users)
            .HasForeignKey(u => u.PositionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<AppUser>()
            .HasOne(u => u.Store)
            .WithMany(s => s.Users)
            .HasForeignKey(u => u.StoreId)
            .OnDelete(DeleteBehavior.SetNull);

        // Order -> AppUser (no cascade)
        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany()
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // Schedule -> AppUser
        builder.Entity<Schedule>()
            .HasOne(s => s.User)
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Timekeeping -> AppUser
        builder.Entity<Timekeeping>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // InventoryLog -> AppUser
        builder.Entity<InventoryLog>()
            .HasOne(l => l.User)
            .WithMany()
            .HasForeignKey(l => l.UserId)
            .OnDelete(DeleteBehavior.SetNull);

        // PurchaseOrder -> Supplier
        builder.Entity<PurchaseOrder>()
            .HasOne(p => p.Supplier)
            .WithMany(s => s.PurchaseOrders)
            .HasForeignKey(p => p.SupplierId)
            .OnDelete(DeleteBehavior.SetNull);

        // Indexes
        builder.Entity<Order>()
            .HasIndex(o => new { o.StoreId, o.OrderDate });
        builder.Entity<Timekeeping>()
            .HasIndex(t => new { t.UserId, t.Date });
        builder.Entity<InventoryLog>()
            .HasIndex(l => new { l.StoreId, l.CreatedAt });
        builder.Entity<PointHistory>()
            .HasIndex(p => p.CustomerId);
    }
}
