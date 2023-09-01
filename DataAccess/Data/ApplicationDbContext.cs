using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Data.Data.Models.Chat;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Data.Data
{
    /// <summary>
    /// Represents the database context for the application's data models.
    /// </summary>
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationDbContext"/> class.
        /// </summary>
        /// <param name="options">The options for configuring the context.</param>
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Canvas> Canvases { get; set; }
        public DbSet<CanvasObject> objects { get; set; }
        public DbSet<BaseObject> BaseObjects { get; set; }

        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Country> Countries { get; set; }
        public DbSet<City> Cities { get; set; }

        public DbSet<ApplicationUser> ApplicationUsers { get; set; }
        public DbSet<Address> Addresses { get; set; }

        public DbSet<SharedProject> SharedProjects { get; set; }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<OrderHeader> OrderHeaders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        /// <summary>
        /// Configures the options for the database context.
        /// </summary>
        /// <param name="optionsBuilder">The options builder for configuring the context.</param>
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;User Id=Dimo;Password=111;Database=Car-Designer;Trusted_Connection=True;");
            }
        }

        /// <summary>
        /// Configures the database context.
        /// </summary>
        /// <param name="modelBuilder">The model builder for configuring the context.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Country)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.City)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CityId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ApplicationUser>()
                .HasMany(u => u.ShoppingCarts)
                .WithOne(sc => sc.ApplicationUser)
                .HasForeignKey(sc => sc.ApplicationUserId)
                .OnDelete(DeleteBehavior.Cascade);


            modelBuilder.Entity<City>()
                .HasOne(c => c.Country)
                .WithMany(c => c.Cities)
                .HasForeignKey(c => c.CountryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.City)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CityId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Address>()
                .HasOne(a => a.Country)
                .WithMany(c => c.Addresses)
                .HasForeignKey(a => a.CountryId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CanvasObject>()
                .HasOne(co => co.root)
                .WithMany(c => c.Objects)
                .HasForeignKey(co => co.rootId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BaseObject>()
                .HasOne(bo => bo.root)
                .WithOne(c => c.BaseObject)
                .HasForeignKey<BaseObject>(bo => bo.rootId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<SharedProject>()
                .HasOne(sp => sp.Canvas)
                .WithOne()
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ShoppingCart>()
                .HasMany(sc => sc.Items)
                .WithOne(ci => ci.ShoppingCart)
                .HasForeignKey(ci => ci.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.Items)
                .WithOne(ci => ci.Product)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CartItem>()
                .HasOne(ci => ci.ShoppingCart)
                .WithMany(sc => sc.Items)
                .HasForeignKey(ci => ci.ShoppingCartId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderHeader>()
                .HasMany(oh => oh.OrderDetails)
                .WithOne(od => od.OrderHeader)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Product>()
                .HasMany(p => p.OrderDetails)
                .WithOne(od => od.Product)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            base.OnModelCreating(modelBuilder);
        }
    }
}