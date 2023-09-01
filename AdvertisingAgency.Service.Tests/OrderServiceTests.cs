using AdvertisingAgency.Data.Data;
using AdvertisingAgency.Data.Data.Models;
using AdvertisingAgency.Services;
using AdvertisingAgency.Services.Interfaces;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace AdvertisingAgency.Service.Tests
{
    [TestFixture]
    public class OrderServiceTests
    {
        private ApplicationDbContext _context;
        private IOrderService _service;
        private string _testUserId = "test_user_id";

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
            _service = new OrderService(_context);
        }

        [Test]
        public async Task GetUserInfo_ReturnsUser_WhenUserExists()
        {
            // Arrange
            var user = new ApplicationUser { Id = _testUserId, Email = "test@test.com" };
            _context.ApplicationUsers.Add(user);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetUserInfo(_testUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(user);
        }

        [Test]
        public async Task GetUserInfo_ThrowsException_WhenUserDoesNotExist()
        {
            // Arrange
            string nonExistentUserId = "non_existent_user_id";

            // Act & Assert
            Func<Task> act = async () => await _service.GetUserInfo(nonExistentUserId);
            await act.Should().ThrowAsync<Exception>().WithMessage("The user was not found");
        }

        [Test]
        public async Task GetOrdersForUser_ReturnsListOfOrderHeaders_WhenOrdersExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };

            // Add the users to the database
            _context.ApplicationUsers.Add(user);

            var orderHeaders = new List<OrderHeader>
        {
            new()
            {
                Id = 1,
                ApplicationUser = user,
                OrderDate = new DateTime(2021, 1, 1),
                OrderTotal = 100,
                Name = "Test Order",
                Address = "Test Address",
                City = new City { Id = 1, Name = "Test City" },
                Country = new Country { Id = 1, Name = "Test Country" },
                PostalCode = "Test Postal Code",
                PhoneNumber = "+359",
            },
        };
            _context.ApplicationUsers.Add(user);
            _context.OrderHeaders.AddRange(orderHeaders);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetOrdersForUser(user.Id);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(orderHeaders);
        }

        [Test]
        public async Task GetOrdersForUser_ReturnsEmptyList_WhenNoOrdersFound()
        {
            // Act
            var result = await _service.GetOrdersForUser(_testUserId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetItemsForOrder_ReturnsListOfOrderDetails_WhenOrderDetailsExist()
        {
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };

            // Add the users to the database
            _context.ApplicationUsers.Add(user);
            // Arrange
            var shoppingCart = new ShoppingCart { Id = 1, ApplicationUser = user, ApplicationUserId = user.Id };

            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                Description = "Test Description",
                Price = 100,
                Author = "John Dow",
                ImageUrl = "https://test.com/test.jpg",
                Category = "Test Category",

            };

            var item = new CartItem
            {
                Id = 1,
                ProductId = 1,
                Quantity = 1,
                ShoppingCartId = 1,
                Thumbnail = "https://test.com/test.jpg",
                ShoppingCart = shoppingCart,
                CanvasId = Guid.NewGuid(),
                Product = product
            };

            var orderHeader = new OrderHeader
            {
                ApplicationUser = user,
                OrderDate = new DateTime(2021, 1, 1),
                OrderDetails = new List<OrderDetail>(),
                OrderTotal = 100,
                Name = "Test Order",
                Address = "Test Address",
                City = new City { Id = 1, Name = "Test City" },
                Country = new Country { Id = 1, Name = "Test Country" },
                PostalCode = "Test Postal Code",
                PhoneNumber = "+359",
            };

            var orderId = 1;
            var orderDetails = new List<OrderDetail>
        {
            new()
            {
                Id = 1,
                OrderId = orderId,
                OrderHeader = orderHeader,
                ProductId = 1,
                Product = product,
                Price = 100,
                Count = 1,
                ProductTitle = "New Order"
            }
        };
            _context.OrderDetails.AddRange(orderDetails);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetItemsForOrder(orderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEquivalentTo(orderDetails);
            result.First().Product.Should().BeEquivalentTo(product);
            result.First().OrderHeader.Should().BeEquivalentTo(orderHeader);
            result.First().OrderHeader.ApplicationUser.Should().BeEquivalentTo(user);
            result.First().OrderHeader.City.Should().BeEquivalentTo(orderHeader.City);
            result.First().OrderHeader.Country.Should().BeEquivalentTo(orderHeader.Country);
            result.First().OrderHeader.OrderDetails.Should().BeEquivalentTo(orderDetails);
            result.First().OrderHeader.OrderTotal.Should().Be(orderHeader.OrderTotal);
            result.First().OrderHeader.PhoneNumber.Should().Be(orderHeader.PhoneNumber);
            result.First().OrderHeader.PostalCode.Should().Be(orderHeader.PostalCode);
            result.Should().HaveCount(1);
        }

        [Test]
        public async Task GetItemsForOrder_ReturnsEmptyList_WhenNoOrderDetailsFound()
        {
            // Arrange
            var nonExistentOrderId = 100;

            // Act
            var result = await _service.GetItemsForOrder(nonExistentOrderId);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetOrdersByDate_ReturnsListOfOrderHeaders_WhenOrdersExistWithinDateRange()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
            var startDate = new DateTime(2021, 1, 1);
            var endDate = new DateTime(2021, 12, 31);

            // Add the users to the database
            _context.ApplicationUsers.Add(user);

            var shoppingCart = new ShoppingCart { Id = 1, ApplicationUser = user, ApplicationUserId = user.Id };
            _context.ShoppingCarts.Add(shoppingCart);

            var product = new Product
            {
                Id = 1,
                Title = "Test Product",
                Description = "Test Description",
                Price = 100,
                Author = "John Dow",
                ImageUrl = "https://test.com/test.jpg",
                Category = "Test Category",
            };
            _context.Products.Add(product);

            var item = new CartItem
            {
                Id = 1,
                ProductId = 1,
                Quantity = 1,
                ShoppingCartId = 1,
                Thumbnail = "https://test.com/test.jpg",
                ShoppingCart = shoppingCart,
                CanvasId = Guid.NewGuid(),
                Product = product
            };
            _context.CartItems.Add(item);

            var orderId = 1;
            var orderHeader = new OrderHeader
            {
                Id = orderId,
                ApplicationUser = user,
                OrderDate = new DateTime(2021, 5, 10),
                OrderDetails = new List<OrderDetail>(),
                OrderTotal = 100,
                Name = "Test Order",
                Address = "Test Address",
                City = new City { Id = 1, Name = "Test City" },
                Country = new Country { Id = 1, Name = "Test Country" },
                PostalCode = "Test Postal Code",
                PhoneNumber = "+359",
            };
            _context.OrderHeaders.Add(orderHeader);

            var orderDetail = new OrderDetail
            {
                Id = 1,
                OrderId = orderId,
                OrderHeader = orderHeader,
                ProductId = 1,
                Product = product,
                Price = 100,
                Count = 1,
                ProductTitle = "New Order"
            };
            _context.OrderDetails.Add(orderDetail);

            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetOrdersByDate(user, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result.Should().Contain(orderHeader);
        }



        [Test]
        public async Task GetOrdersByDate_ReturnsEmptyList_WhenUserIsNull()
        {
            // Arrange
            ApplicationUser user = null;
            var startDate = new DateTime(2021, 1, 1);
            var endDate = new DateTime(2021, 12, 31);

            // Act
            var result = await _service.GetOrdersByDate(user, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetOrdersByDate_ReturnsEmptyList_WhenNoOrdersExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
            var startDate = new DateTime(2021, 1, 1);
            var endDate = new DateTime(2021, 12, 31);

            // Act
            var result = await _service.GetOrdersByDate(user, startDate, endDate);

            // Assert
            result.Should().NotBeNull();
            result.Should().BeEmpty();
        }

        [Test]
        public async Task GetOrderData_ReturnsListOfCanvasObjects_WhenCanvasAndObjectsExist()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
            var canvasId = Guid.NewGuid();

            // Add sample canvas with objects
            var canvas = new Canvas
            {
                Id = canvasId, 
                Objects = new List<CanvasObject> { new CanvasObject { Id = 1 }},
                User = user,
                UserId = user.UserId,
                Name = "John Dow"
            };
            _context.Canvases.Add(canvas);
            await _context.SaveChangesAsync();

            // Act
            var result = await _service.GetOrderData(user, canvasId);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
        }

        [Test]
        public async Task GetOrderData_ThrowsException_WhenCanvasNotFound()
        {
            // Arrange
            var user = new ApplicationUser { Id = Guid.NewGuid().ToString(), Email = "test8@test.com", UserName = "Test User" };
            var canvasId = Guid.NewGuid();

            // Act
            Func<Task> act = async () => await _service.GetOrderData(user, canvasId);

            // Assert
            await act.Should().ThrowAsync<Exception>()
                .WithMessage("Canvas not found");
        }
    }

}
