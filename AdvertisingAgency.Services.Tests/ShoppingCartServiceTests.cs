using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using Xunit;
using Assert = NUnit.Framework.Assert;

namespace AdvertisingAgency.Services.Tests
{
    [TestFixture]
    public class ShoppingCartServiceTests
    {
        private ApplicationDbContext _context;
        private ShoppingCartService _service;

        public ShoppingCartServiceTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new ApplicationDbContext(options);
            _service = new ShoppingCartService(_context);
        }

        [Fact]
        public async Task GetCartAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {
            // Arrange
            var service = new ShoppingCartService(_context);

            // Assert
            Assert.ThrowsAsync<ArgumentNullException>(() => service.GetCartAsync(null));
        }


        [Fact]
        public async Task GetCartAsync_ReturnsNull_WhenUserIsNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetCartAsync(null));
        }

        [Fact]
        public async Task GetCartAsync_ReturnsCart_WhenUserExists()
        {
            var user = new ApplicationUser { Id = "1", UserName = "Test" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };

            _context.Users.Add(user);
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartAsync(user);

            Assert.AreEqual(cart, result);
        }

        [Fact]
        public async Task GetCartItemCountAsync_ReturnsZero_WhenNoCartExists()
        {
            var user = new ApplicationUser { Id = "1", UserName = "Test" };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemCountAsync(user);

            Assert.AreEqual(0, result);
        }

        [Fact]
        public async Task GetCartItemCountAsync_ReturnsItemCount_WhenCartExists()
        {
            var user = new ApplicationUser { Id = "1", UserName = "Test" };
            var product = new Product { Id = 1, Price = 10 };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 2 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemCountAsync(user);

            Assert.AreEqual(2, result);
        }

        [Fact]
        public async Task AddToCartAsync_AddsItemToNewCart_WhenCartDoesNotExist()
        {
            var user = new ApplicationUser { Id = "1", UserName = "Test" };
            var productDescription = "Product Description";
            var productsPrice = 10.0;
            var categoryId = 1;
            var canvasId = Guid.NewGuid();
            var thumbnail = "thumbnail";
            var canvasName = "Canvas Name";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _service.AddToCartAsync(productsPrice, productDescription, categoryId, user, user.UserName, null, canvasName, canvasId, thumbnail);

            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync();
            var cartItem = await _context.CartItems.FirstOrDefaultAsync();

            Assert.NotNull(cart);
            Assert.NotNull(cartItem);
            Assert.AreEqual(productDescription, cartItem.Product.Description);
            Assert.AreEqual(1, cart.Items.Count);
            Assert.AreEqual(productsPrice, cart.Price);
        }

        [Fact]
        public async Task GetCartItemsAsync_ReturnsEmpty_WhenNoItemsInCart()
        {
            var user = new ApplicationUser { Id = "1", UserName = "Test" };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemsAsync(user);

            Assert.IsEmpty(result);
        }

        [Fact]
        public async Task GetCartItemsAsync_ReturnsItems_WhenItemsInCart()
        {
            var user = new ApplicationUser { Id = "1", UserName = "Test" };
            var product = new Product { Id = 1, Price = 10 };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 2 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemsAsync(user);

            Assert.IsNotEmpty(result);
            Assert.AreEqual(product, result.First().Product);
        }

        [Fact]
        public async Task ClearCartAsync_RemovesCart1()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1" };
            var cart = new ShoppingCart { ApplicationUser = user };
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            var service = new ShoppingCartService(_context);

            // Act
            await service.ClearCartAsync(user);

            // Assert
            var result = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.ApplicationUser.Id == user.Id);
            Assert.Null(result);
        }

        [Fact]
        public async Task ClearCartAsync_RemovesCart2()
        {
            var user = new ApplicationUser { Id = "1", UserName = "Test" };
            var product = new Product { Id = 1, Price = 10 };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 2 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            await _service.ClearCartAsync(user);

            var cartInDb = await _context.ShoppingCarts.FirstOrDefaultAsync();

            Assert.Null(cartInDb);
        }

        [Fact]
        public async Task GetCartItemCountAsync_ReturnsCorrectItemCount()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1" };
            var cart = new ShoppingCart
            {
                ApplicationUser = user,
                Items = new List<CartItem>
                {
                    new CartItem { Quantity = 2 },
                    new CartItem { Quantity = 3 }
                }
            };
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            var service = new ShoppingCartService(_context);

            // Act
            var result = await service.GetCartItemCountAsync(user);

            // Assert
            Assert.AreEqual(5, result);
        }

        [Fact]
        public async Task AddToCartAsync_CreatesNewCart_WhenNoCartExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = "1" };
            var service = new ShoppingCartService(_context);

            // Act
            await service.AddToCartAsync(100, "Product Description", 1, user, "UserName", "Url", "CanvasName", Guid.NewGuid(), "Thumbnail");

            // Assert
            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.ApplicationUser.Id == user.Id);
            Assert.NotNull(cart);
        }
    }
}
