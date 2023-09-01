using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace AdvertisingAgency.Services.Tests
{
    [TestFixture]
    public class ShoppingCartServiceTests1
    {
        // Test data
        private List<ApplicationUser> users;
        private List<ShoppingCart> shoppingCarts;
        private List<Product> products;
        private List<CartItem> cartItems;

        [SetUp]
        public void SetUp()
        {
            // Initialize test data
            users = new List<ApplicationUser>
        {
            new ApplicationUser { Id = "user1", PhoneNumberConfirmed = true, FriendlyName = "John Doe", AddressId = 1, CityId = 1, CountryId = 1 },
            new ApplicationUser { Id = "user2", PhoneNumberConfirmed = false, FriendlyName = "Jane Smith", AddressId = 2, CityId = 2, CountryId = 2 },
        };

            shoppingCarts = new List<ShoppingCart>
        {
            new ShoppingCart { Id = 1, ApplicationUser = users[0], Items = new List<CartItem>() },
            new ShoppingCart { Id = 2, ApplicationUser = users[1], Items = new List<CartItem>() },
        };

            products = new List<Product>
        {
            new Product { Id = 1, Title = "Product 1", Price = 10.00 },
            new Product { Id = 2, Title = "Product 2", Price = 20.00 },
        };

            cartItems = new List<CartItem>
        {
            new CartItem { Id = 1, Product = products[0], Quantity = 2 },
            new CartItem { Id = 2, Product = products[1], Quantity = 1 },
        };
        }

        private DbSet<T> GetQueryableMockDbSet<T>(List<T> sourceList) where T : class
        {
            var queryable = sourceList.AsQueryable();
            var dbSet = new Mock<DbSet<T>>();
            dbSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
            dbSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
            dbSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
            dbSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => queryable.GetEnumerator());
            dbSet.Setup(d => d.Add(It.IsAny<T>())).Callback<T>((s) => sourceList.Add(s));
            return dbSet.Object;
        }

        [Test]
        public async Task GetCartAsync_UserNotNull_ReturnsCart()
        {
            // Arrange
            var dbContextMock = new Mock<ApplicationDbContext>();
            var user = users[0];
            dbContextMock.Setup(m => m.ShoppingCarts).Returns(GetQueryableMockDbSet(shoppingCarts));

            var shoppingCartService = new ShoppingCartService(dbContextMock.Object);

            // Act
            var result = await shoppingCartService.GetCartAsync(user);

            // Assert
            Assert.AreEqual(shoppingCarts[0], result);
        }

        [Test]
        public async Task GetCartAsync_UserNull_ThrowsArgumentNullException()
        {
            // Arrange
            var dbContextMock = new Mock<ApplicationDbContext>();
            var shoppingCartService = new ShoppingCartService(dbContextMock.Object);

            // Act and Assert
            Assert.ThrowsAsync<ArgumentNullException>(async () => await shoppingCartService.GetCartAsync(null));
        }

        [Test]
        public async Task GetCartItemCountAsync_CartExists_ReturnsItemCount()
        {
            // Arrange
            var dbContextMock = new Mock<ApplicationDbContext>();
            var user = users[0];
            var cart = shoppingCarts[0];
            cart.Items = cartItems;

            dbContextMock.Setup(m => m.ShoppingCarts).Returns(GetQueryableMockDbSet(shoppingCarts));
            var shoppingCartService = new ShoppingCartService(dbContextMock.Object);

            // Act
            var result = await shoppingCartService.GetCartItemCountAsync(user);

            // Assert
            Assert.AreEqual(cartItems.Sum(i => i.Quantity), result);
        }

        [Test]
        public async Task GetCartItemCountAsync_CartDoesNotExist_ReturnsZero()
        {
            // Arrange
            var dbContextMock = new Mock<ApplicationDbContext>();
            var user = users[0];

            dbContextMock.Setup(m => m.ShoppingCarts).Returns(GetQueryableMockDbSet(shoppingCarts));
            var shoppingCartService = new ShoppingCartService(dbContextMock.Object);

            // Act
            var result = await shoppingCartService.GetCartItemCountAsync(user);

            // Assert
            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task AddToCartAsync_ItemDoesNotExistInCart_CreateNewCartItem()
        {
            // Arrange
            var dbContextMock = new Mock<ApplicationDbContext>();
            var user = users[0];
            var product = products[0];
            var cart = shoppingCarts[0];
            cart.Items.Clear();

            dbContextMock.Setup(m => m.Products).Returns(GetQueryableMockDbSet(products));
            dbContextMock.Setup(m => m.ShoppingCarts).Returns(GetQueryableMockDbSet(shoppingCarts));
            dbContextMock.Setup(m => m.Canvases).Returns(GetQueryableMockDbSet(new List<Canvas>()));

            var shoppingCartService = new ShoppingCartService(dbContextMock.Object);

            // Act
            await shoppingCartService.AddToCartAsync(product.Price, product.Description, product.CategoryId, user, user.FriendlyName, null, product.Title, product.CanvasId, null);

            // Assert
            Assert.AreEqual(1, cart.Items.Count);
            var cartItem = cart.Items.Single();
            Assert.AreEqual(product.Title, cartItem.Product.Title);
            Assert.AreEqual(1, cartItem.Quantity);
        }


        [Test]
        public async Task AddToCartAsync_ItemExistsInCart_IncreaseQuantity()
        {
            // Arrange
            var dbContextMock = new Mock<ApplicationDbContext>();
            var user = users[0];
            var product = products[0];
            var cart = shoppingCarts[0];
            cart.Items = new List<CartItem> { cartItems[0] };

            dbContextMock.Setup(m => m.Products).Returns(GetQueryableMockDbSet(products));
            dbContextMock.Setup(m => m.ShoppingCarts).Returns(GetQueryableMockDbSet(shoppingCarts));
            dbContextMock.Setup(m => m.Canvases).Returns(GetQueryableMockDbSet(new List<Canvas>()));

            var shoppingCartService = new ShoppingCartService(dbContextMock.Object);

            // Act
            await shoppingCartService.AddToCartAsync(product.Price, product.Description, product.CategoryId, user, user.FriendlyName, null, product.Title, product.CanvasId, null);

            // Assert
            var existingCartItem = cart.Items.FirstOrDefault(i => i.Product.Title == product.Title);
            Assert.NotNull(existingCartItem);
            Assert.AreEqual(3, existingCartItem.Quantity);
        }

    }

}
