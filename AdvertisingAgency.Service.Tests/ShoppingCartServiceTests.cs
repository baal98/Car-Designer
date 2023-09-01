using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using AdvertisingAgency.Services.Interfaces;
using AdvertisingAgency.Web.ViewModels.ViewModels;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Service.Tests
{
    [TestFixture]
    public class ShoppingCartServiceTests
    {
        private ApplicationDbContext _context;
        private IShoppingCartService _service;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _service = new ShoppingCartService(_context);
        }

        [Test]
        public async Task GetCartAsync_ThrowsArgumentNullException_WhenUserIsNull()
        {
            Assert.ThrowsAsync<ArgumentNullException>(() => _service.GetCartAsync(null));
        }

        [Test]
        public async Task GetCartAsync_ReturnsCart_WhenUserExists()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test1@test.com" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };

            _context.Users.Add(user);
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartAsync(user);

            Assert.AreEqual(cart, result);
        }

        [Test]
        public async Task GetCartItemCountAsync_ReturnsCorrectItemCount_WhenCartExists()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test3@test.com" };
            var product = new Product { Id = 1, Price = 10, Author = "Test Author" };
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

        [Test]
        public async Task GetCartItemCountAsync_ReturnsZero_WhenNoCartExists()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test2@test.com" };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemCountAsync(user);

            Assert.AreEqual(0, result);
        }

        [Test]
        public async Task GetCartItemCountAsync_ReturnsCorrectItemCount_WhenMultipleProductsInCart()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test7@test.com" };
            var product1 = new Product { Id = 1, Price = 10, Author = "Author Name" };
            var product2 = new Product { Id = 2, Price = 10, Author = "Author Name" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem1 = new CartItem { Id = 1, Product = product1, ShoppingCart = cart, Quantity = 2 };
            var cartItem2 = new CartItem { Id = 2, Product = product2, ShoppingCart = cart, Quantity = 3 };

            cart.Items.Add(cartItem1);
            cart.Items.Add(cartItem2);
            _context.Users.Add(user);
            _context.Products.AddRange(new[] { product1, product2 });
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.AddRange(new[] { cartItem1, cartItem2 });
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemCountAsync(user);

            Assert.AreEqual(5, result);
        }

        [Test]
        public async Task AddToCartAsync_AddsItemToNewCart_WhenCartDoesNotExist()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
            var productDescription = "Product Description";
            var productsPrice = 10.0;
            var categoryId = 1;
            var canvasId = Guid.NewGuid();
            var thumbnail = "thumbnail";
            var canvasName = "Canvas Name";
            var url = "http://example.com";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            await _service.AddToCartAsync(productsPrice, productDescription, categoryId, user, user.UserName, url, canvasName, canvasId, thumbnail);
            var cart = _context.ShoppingCarts.FirstOrDefault(c => c.ApplicationUser == user);

            Assert.NotNull(cart);
            Assert.AreEqual(1, cart.Items.Count);
        }

        [Test]
        public async Task AddToCartAsync_IncreasesQuantity_WhenItemAlreadyExistsInCart()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
            var productDescription = "Product Description";
            var productsPrice = 10.0;
            var categoryId = 1;
            var canvasId = Guid.NewGuid();
            var thumbnail = "thumbnail";
            var canvasName = "Canvas Name";
            var url = "http://example.com";

            var existingProduct = new Product
            {
                Title = canvasName,
                CanvasId = canvasId,
                Price = productsPrice,
                Author = "Jon Dow"
            };

            var existingCartItem = new CartItem
            {
                Product = existingProduct,
                Thumbnail = thumbnail,
                Quantity = 1, // Set initial quantity to 1
                CanvasId = canvasId
            };

            var existingCart = new ShoppingCart
            {
                ApplicationUser = user,
                Items = new List<CartItem> { existingCartItem },
                Price = existingCartItem.Product.Price * existingCartItem.Quantity // Set initial cart price based on the existing item
            };

            _context.Users.Add(user);
            _context.Products.Add(existingProduct);
            _context.ShoppingCarts.Add(existingCart);
            await _context.SaveChangesAsync();

            // Act
            await _service.AddToCartAsync(productsPrice, productDescription, categoryId, user, user.UserName, url, canvasName, canvasId, thumbnail);

            // Assert
            var updatedCart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.ApplicationUser == user);
            var updatedCartItem = updatedCart?.Items?.FirstOrDefault(i => i.Product.Title == canvasName && i.Thumbnail == thumbnail);

            Assert.NotNull(updatedCart);
            Assert.NotNull(updatedCartItem);

            Assert.AreEqual(1, updatedCart.Items.Count); // Only one item should be in the cart
            Assert.AreEqual(2, updatedCartItem.Quantity); // Quantity of the existing item should be increased by 1
            Assert.AreEqual(productsPrice * 2, updatedCart.Price); // Cart's total price should be updated based on the new quantity
        }


        [Test]
        public async Task AddToCartAsync_AddsNewItemToExistingCart_WhenItemDoesNotExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
            var productDescription = "Product Description";
            var productsPrice = 10.0;
            var categoryId = 1;
            var canvasId = Guid.NewGuid();
            var thumbnail = "thumbnail";
            var canvasName = "Canvas Name";
            var url = "http://example.com";

            var existingProduct = new Product
            {
                Title = "Existing Product",
                CanvasId = Guid.NewGuid(),
                Price = 20.0,
                Author = "Jon Dow"
            };

            var existingCartItem = new CartItem
            {
                Product = existingProduct,
                Thumbnail = "existingThumbnail",
                Quantity = 2,
                CanvasId = existingProduct.CanvasId
            };

            var existingCart = new ShoppingCart
            {
                ApplicationUser = user,
                Items = new List<CartItem> { existingCartItem },
                Price = existingCartItem.Quantity * existingProduct.Price
            };

            _context.Users.Add(user);
            _context.Products.Add(existingProduct);
            _context.ShoppingCarts.Add(existingCart);
            await _context.SaveChangesAsync();

            // Act
            await _service.AddToCartAsync(productsPrice, productDescription, categoryId, user, user.UserName, url, canvasName, canvasId, thumbnail);

            // Assert
            var updatedCart = await _context.ShoppingCarts.FirstOrDefaultAsync(c => c.ApplicationUser == user);
            var updatedCartItem = updatedCart?.Items?.FirstOrDefault(i => i.Product.Title == canvasName && i.Thumbnail == thumbnail);

            Assert.NotNull(updatedCart);
            Assert.NotNull(updatedCartItem);

            Assert.AreEqual(2, updatedCart.Items.Count);
            Assert.AreEqual(1, updatedCartItem.Quantity);
            Assert.AreEqual(productsPrice + existingCartItem.Product.Price * existingCartItem.Quantity, updatedCart.Price);
        }

        [Test]
        public async Task GetCartItemsAsync_ReturnsEmpty_WhenNoItemsInCart()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test9@test.com" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };

            _context.Users.Add(user);
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemsAsync(user);

            Assert.IsEmpty(result);
        }

        [Test]
        public async Task GetCartItemsAsync_ReturnsItems_WhenItemsInCart()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test10@test.com" };
            var product = new Product { Id = 1, Price = 10, Author = "Test Author" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 2 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var result = await _service.GetCartItemsAsync(user);

            Assert.AreEqual(1, result.Count());
            Assert.AreEqual(cartItem, result.First());
        }

        [Test]
        public async Task ClearCartAsync_DoesNotRemoveOtherUsersCart()
        {
            var user1 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test11@test.com" };
            var user2 = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test12@test.com" };
            var cart1 = new ShoppingCart { Id = 1, ApplicationUser = user1 };
            var cart2 = new ShoppingCart { Id = 2, ApplicationUser = user2 };

            _context.Users.AddRange(new[] { user1, user2 });
            _context.ShoppingCarts.AddRange(new[] { cart1, cart2 });
            await _context.SaveChangesAsync();

            await _service.ClearCartAsync(user1);
            var result = await _service.GetCartAsync(user2);

            Assert.NotNull(result);
            Assert.AreEqual(cart2, result);
        }

        [Test]
        public async Task ClearCartAsync_DoesNotThrowException_WhenCartIsEmpty()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test13@test.com" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };

            _context.Users.Add(user);
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            await _service.ClearCartAsync(user);
            var result = await _service.GetCartAsync(user);

            Assert.IsNull(result);
        }

        [Test]
        public async Task ClearCartAsync_RemovesCart()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test14@test.com" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };

            _context.Users.Add(user);
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            await _service.ClearCartAsync(user);
            var result = await _service.GetCartAsync(user);

            Assert.Null(result);
        }

        [Test]
        public async Task DecrementCartItemQuantityAsync_DecreasesQuantity_WhenQuantityIsMoreThanOne()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test15@test.com" };
            var product = new Product { Id = 1, Price = 10, Author = "Test Author" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 2 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            await _service.DecrementCartItemQuantityAsync(cartItem.Id, user);

            Assert.AreEqual(1, cartItem.Quantity);
        }

        [Test]
        public async Task DecrementCartItemQuantityAsync_RemovesItem_WhenQuantityIsOne()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test16@test.com" };
            var product = new Product { Id = 1, Price = 10, Author = "Test Author" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 1 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            await _service.DecrementCartItemQuantityAsync(cartItem.Id, user);

            Assert.IsEmpty(cart.Items);
        }

        [Test]
        public async Task IncrementCartItemQuantityAsync_IncreasesQuantity()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test17@test.com" };
            var product = new Product { Id = 1, Price = 10, Author = "Test Author" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 1 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            await _service.IncrementCartItemQuantityAsync(cartItem.Id, user);

            Assert.AreEqual(2, cartItem.Quantity);
        }

        [Test]
        public async Task RemoveCartItemAsync_RemovesItem()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test18@test.com" };
            var product = new Product { Id = 1, Price = 10, Author = "Test Author" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 1 };

            cart.Items.Add(cartItem);
            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            await _service.RemoveCartItemAsync(cartItem.Id, user);

            Assert.IsEmpty(cart.Items);
        }

        [Test]
        public async Task CheckUserProfileComplete_ReturnsIncomplete_WhenProfileIsIncomplete()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@test.com",
                PhoneNumberConfirmed = false
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (isComplete, message) = await _service.CheckUserProfileComplete(user);

            Assert.False(isComplete);
            Assert.AreEqual("Вашият профил не е пълен. Моля, актуализирайте профила си преди да направите поръчка.", message);
        }

        [Test]
        public async Task CheckUserProfileComplete_ReturnsComplete_WhenProfileIsComplete()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@test.com",
                PhoneNumberConfirmed = true,
                AddressId = 1,
                CityId = 1,
                CountryId = 1,
                FriendlyName = "Test User"
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var (isComplete, message) = await _service.CheckUserProfileComplete(user);

            Assert.True(isComplete);
            Assert.AreEqual("Профилът е пълен.", message);
        }

        [Test]
        public async Task CreateOrderAsync_CreatesOrder_WhenCartExists()
        {
            var userId = Guid.NewGuid().ToString();

            var user = new ApplicationUser
            {
                Id = userId,
                Email = "test@test.com",
                Address = new Address
                {
                    UserId = userId,
                    Street = "Test Street",
                    BuildingNumber = "Test Building Number"
                },
                Country = new Country
                {
                    Name = "Test Country"
                },
                City = new City
                {
                    Name = "Test City"
                },
                FriendlyName = "Test User"
            };

            var product = new Product { Id = 1, Price = 10, Author = "Test Author" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            var cartItem = new CartItem { Id = 1, Product = product, ShoppingCart = cart, Quantity = 1 };
            cart.Items.Add(cartItem);

            var shoppingCartVM = new shoppingCartVM();

            _context.Users.Add(user);
            _context.Products.Add(product);
            _context.ShoppingCarts.Add(cart);
            _context.CartItems.Add(cartItem);
            await _context.SaveChangesAsync();

            var result = await _service.CreateOrderAsync(user, shoppingCartVM);

            Assert.True(result);
            Assert.AreEqual(1, _context.OrderHeaders.Count());
        }

        [Test]
        public async Task CreateOrderAsync_ReturnsFalse_WhenCartDoesNotExist()
        {
            var user = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                Email = "test@test.com"
            };

            var shoppingCartVM = new shoppingCartVM();

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            var result = await _service.CreateOrderAsync(user, shoppingCartVM);

            Assert.False(result);
            Assert.AreEqual(0, _context.OrderHeaders.Count());
        }

        [Test]
        public async Task RemoveCartAsync_RemovesCart_WhenCartExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test@test.com" };
            var cart = new ShoppingCart { Id = 1, ApplicationUser = user };
            _context.Users.Add(user);
            _context.ShoppingCarts.Add(cart);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.RemoveCartAsync(user);

            // Assert
            Assert.True(result);
            Assert.AreEqual(0, _context.ShoppingCarts.Count());
        }

        [Test]
        public async Task RemoveCartAsync_ReturnsFalse_WhenCartDoesNotExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test@test.com" };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.RemoveCartAsync(user);

            // Assert
            Assert.False(result);
        }

        [Test]
        public async Task GetAllUsersWithOrdersAsync_ReturnsUsersWithOrders()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test@test.com", FriendlyName = "Test User" };
            var orderHeader = new OrderHeader { Id = 1, ApplicationUser = user, Address = "Test Address", Name = "Test Name" };
            var orderDetail = new OrderDetail { Id = 1, OrderHeader = orderHeader, Count = 1, Price = 10 };
            orderHeader.OrderDetails.Add(orderDetail);
            _context.Users.Add(user);
            _context.OrderHeaders.Add(orderHeader);
            _context.OrderDetails.Add(orderDetail);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetAllUsersWithOrdersAsync();

            // Assert
            Assert.AreEqual(1, result.Count);
            if (result.Count > 0)
            {
                var userOrdersViewModel = result[0];
                Assert.AreEqual(user.FriendlyName, userOrdersViewModel.FriendlyName);
                if (userOrdersViewModel.Orders.Count > 0)
                {
                    var orderViewModel = userOrdersViewModel.Orders[0];
                    Assert.AreEqual(orderHeader.Id, orderViewModel.OrderId);
                    if (orderViewModel.OrderDetails.Count > 0)
                    {
                        var orderDetailViewModel = orderViewModel.OrderDetails[0];
                        Assert.AreEqual(orderDetail.Id, orderDetailViewModel.ProductId);
                    }
                }
            }
        }

        [Test]
        public async Task GetUserOrdersAsync_ShouldReturnNullWhenNoOrderHeadersExist()
        {
            // Arrange
            var user1 = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid(),
                FriendlyName = "TestUser1",
                Email = "testuser1@example.com"
            };

            _context.ApplicationUsers.Add(user1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserOrdersAsync(user1.UserId.ToString());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetUserOrdersAsync_ShouldReturnNullWhenNoOrderDetailsExist()
        {
            // Arrange
            var user1 = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.NewGuid(),
                FriendlyName = "TestUser1",
                Email = "testuser1@example.com"
            };

            var orderHeader1 = new OrderHeader
            {
                Id = 1,
                OrderDate = new DateTime(2023, 7, 15),
                OrderTotal = 200,
                ApplicationUser = user1,
                Address = "Test address",
                Name = "Test name"
            };

            _context.ApplicationUsers.Add(user1);
            _context.OrderHeaders.Add(orderHeader1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserOrdersAsync(user1.UserId.ToString());

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetUserOrders_ShouldReturnOrdersForGivenUser()
        {
            // Arrange
            string testUserId = Guid.NewGuid().ToString();

            // Add data to InMemory Database
            var user1 = new ApplicationUser { Id = testUserId, UserId = Guid.NewGuid(), FriendlyName = "TestUser1", Email = "testuser1@example.com" };
            var orderHeader1 = new OrderHeader { Id = 1, OrderDate = new DateTime(2023, 7, 15), OrderTotal = 200, ApplicationUser = user1, Name = "TestName", Address = "TestAddress" };
            _context.ApplicationUsers.Add(user1);
            _context.OrderHeaders.Add(orderHeader1);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserOrders(testUserId);

            // Assert
            result.Should().HaveCount(1);
            result[0].Orders.Should().ContainSingle();
            result[0].Orders[0].OrderId.Should().Be(orderHeader1.Id);
        }

        [Test]
        public async Task GetUserOrdersAsync_ShouldReturnOrdersForGivenUser_WhenUserExists()
        {
            // Arrange
            var testUserId = Guid.NewGuid();
            string testUserStringId = testUserId.ToString();

            // Create and add an ApplicationUser
            var user = new ApplicationUser
            {
                Id = testUserStringId,
                UserId = testUserId,
                FriendlyName = "TestUser",
                Email = "testuser@example.com",
            };
            _context.ApplicationUsers.Add(user);

            // Create and add an OrderHeader with related OrderDetails
            var orderHeader = new OrderHeader
            {
                Id = 1,
                ApplicationUser = user,
                OrderDate = DateTime.Now,
                OrderTotal = 100,
                Address = "TestAddress",
                Name = "TestName",
            };

            var orderDetail = new OrderDetail
            {
                Id = 1,
                OrderHeader = orderHeader,
            };
            orderHeader.OrderDetails = new List<OrderDetail> { orderDetail };
            _context.OrderHeaders.Add(orderHeader);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserOrdersAsync(testUserStringId);

            // Assert
            result.Should().NotBeNull();
            result.Orders.Should().ContainSingle();
            result.Orders[0].OrderId.Should().Be(orderHeader.Id);
        }

        [Test]
        public async Task GetUserOrdersAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            string testUserId = "nonExistingUserId";

            // Act
            var result = await _service.GetUserOrdersAsync(testUserId);

            // Assert
            result.Should().BeNull();
        }

        [Test]
        public async Task GetUserSortedOrdersAsync_ShouldReturnSortedOrdersForGivenUserAndDateRange()
        {
            // Arrange
            DateTime startDate = new DateTime(2023, 7, 1);
            DateTime endDate = new DateTime(2023, 7, 31);

            // Create user and order data
            var city = new City
            {
                Name = "Vratsa" // Set the Name for the City
            };

            var country = new Country
            {
                Name = "Bulgaria" // Set the Name for the Country
            };

            var user1 = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.Parse("6492739B-168A-4E58-A875-03480E716EA1"),
                FriendlyName = "TestUser1",
                Email = "test@example.com",
                PhoneNumber = "+359879641776",
                City = city, // Associate the City with the ApplicationUser
                Country = country // Associate the Country with the ApplicationUser
            };

            var address = new Address
            {
                UserId = user1.UserId.ToString(),
                Street = "123 Street",
                CityId = city.Id,
                City = city,
                CountryId = country.Id,
                Country = country
            };

            var orderHeader1 = new OrderHeader
            {
                Id = 1,
                OrderDate = new DateTime(2023, 7, 15),
                OrderTotal = 200,
                ApplicationUser = user1,
                Address = address.Street,
                Name = "TestOrder1"
            };

            var product1 = new Product
            {
                Id = 101,
                Title = "Product 1",
                Description = "Description for Product 1",
                Price = 50.0,
                Author = "Jon Dow"
            };

            var orderDetail1 = new OrderDetail
            {
                Id = 201,
                OrderId = orderHeader1.Id,
                OrderHeader = orderHeader1,
                ProductId = product1.Id,
                Product = product1,
                Count = 2,
                Price = product1.Price,
                ProductTitle = product1.Title,
                ProductDescription = product1.Description,
                ProductPrice = product1.Price
            };

            orderHeader1.OrderDetails.Add(orderDetail1);

            _context.Cities.Add(city);
            _context.Countries.Add(country);
            _context.ApplicationUsers.Add(user1);
            _context.Addresses.Add(address);
            _context.Products.Add(product1);
            _context.OrderHeaders.Add(orderHeader1);
            _context.OrderDetails.Add(orderDetail1);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserSortedOrdersAsync(user1.UserId.ToString(), startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Orders.Should().ContainSingle();
            result.Orders[0].OrderId.Should().Be(orderHeader1.Id);
        }

        [Test]
        public async Task GetUserSortedOrdersAsync_ShouldReturnSortedOrdersForGivenDateRange()
        {
            // Arrange
            DateTime startDate = new DateTime(2023, 7, 1);
            DateTime endDate = new DateTime(2023, 7, 31);

            // Create users and order data
            var city1 = new City
            {
                Name = "Vratsa"
            };

            var city2 = new City
            {
                Name = "Sofia"
            };

            var country1 = new Country
            {
                Name = "Bulgaria"
            };

            var country2 = new Country
            {
                Name = "USA"
            };

            var user1 = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.Parse("6492739B-168A-4E58-A875-03480E716EA1"),
                FriendlyName = "TestUser1",
                Email = "test1@example.com",
                PhoneNumber = "+359879641776",
                City = city1,
                Country = country1
            };

            var user2 = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserId = Guid.Parse("6492739B-168A-4E58-A875-03480E716EA2"),
                FriendlyName = "TestUser2",
                Email = "test2@example.com",
                PhoneNumber = "+359879641777",
                City = city2,
                Country = country2
            };

            var address1 = new Address
            {
                UserId = user1.UserId.ToString(),
                Street = "123 Street",
                CityId = city1.Id,
                City = city1,
                CountryId = country1.Id,
                Country = country1
            };

            var address2 = new Address
            {
                UserId = user2.UserId.ToString(),
                Street = "456 Street",
                CityId = city2.Id,
                City = city2,
                CountryId = country2.Id,
                Country = country2
            };

            var orderHeader1 = new OrderHeader
            {
                Id = 1,
                OrderDate = new DateTime(2023, 7, 15),
                OrderTotal = 200,
                ApplicationUser = user1,
                Address = address1.Street,
                Name = "TestOrder1"
            };

            var orderHeader2 = new OrderHeader
            {
                Id = 2,
                OrderDate = new DateTime(2023, 7, 20),
                OrderTotal = 150,
                ApplicationUser = user2,
                Address = address2.Street,
                Name = "TestOrder2"
            };

            var product1 = new Product
            {
                Id = 101,
                Title = "Product 1",
                Description = "Description for Product 1",
                Price = 50.0,
                Author = "Jon Dow"
            };

            var product2 = new Product
            {
                Id = 102,
                Title = "Product 2",
                Description = "Description for Product 2",
                Price = 75.0,
                Author = "Jane Smith"
            };

            var orderDetail1 = new OrderDetail
            {
                Id = 201,
                OrderId = orderHeader1.Id,
                OrderHeader = orderHeader1,
                ProductId = product1.Id,
                Product = product1,
                Count = 2,
                Price = product1.Price,
                ProductTitle = product1.Title,
                ProductDescription = product1.Description,
                ProductPrice = product1.Price
            };

            var orderDetail2 = new OrderDetail
            {
                Id = 202,
                OrderId = orderHeader2.Id,
                OrderHeader = orderHeader2,
                ProductId = product2.Id,
                Product = product2,
                Count = 1,
                Price = product2.Price,
                ProductTitle = product2.Title,
                ProductDescription = product2.Description,
                ProductPrice = product2.Price
            };

            orderHeader1.OrderDetails.Add(orderDetail1);
            orderHeader2.OrderDetails.Add(orderDetail2);

            _context.Cities.Add(city1);
            _context.Cities.Add(city2);
            _context.Countries.Add(country1);
            _context.Countries.Add(country2);
            _context.ApplicationUsers.Add(user1);
            _context.ApplicationUsers.Add(user2);
            _context.Addresses.Add(address1);
            _context.Addresses.Add(address2);
            _context.Products.Add(product1);
            _context.Products.Add(product2);
            _context.OrderHeaders.Add(orderHeader1);
            _context.OrderHeaders.Add(orderHeader2);
            _context.OrderDetails.Add(orderDetail1);
            _context.OrderDetails.Add(orderDetail2);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserSortedOrdersAsync(startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2); // There are two users with orders in the given date range
            result[0].UserId.Should().Be(user1.UserId.ToString());
            result[0].FriendlyName.Should().Be(user1.FriendlyName);
            result[0].Orders.Should().HaveCount(1); // User1 has one order in the given date range
            result[0].Orders[0].OrderId.Should().Be(orderHeader1.Id);
            result[0].Orders[0].OrderDetails.Should().HaveCount(1); // OrderHeader1 has one order detail
            result[0].Orders[0].OrderDetails[0].ProductId.Should().Be(product1.Id);

            result[1].UserId.Should().Be(user2.UserId.ToString());
            result[1].FriendlyName.Should().Be(user2.FriendlyName);
            result[1].Orders.Should().HaveCount(1); // User2 has one order in the given date range
            result[1].Orders[0].OrderId.Should().Be(orderHeader2.Id);
            result[1].Orders[0].OrderDetails.Should().HaveCount(1); // OrderHeader2 has one order detail
            result[1].Orders[0].OrderDetails[0].ProductId.Should().Be(product2.Id);
        }

    }
}
