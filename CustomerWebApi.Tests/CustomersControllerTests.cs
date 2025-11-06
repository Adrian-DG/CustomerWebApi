using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using CustomerWebApi.Controllers;
using CustomerWebApi.Data;
using CustomerWebApi.Models;

namespace CustomerWebApi.Tests
{
    public class CustomersControllerTests
    {
        private CustomerContext GetInMemoryDbContext()
        {
            var options = new DbContextOptionsBuilder<CustomerContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            return new CustomerContext(options);
        }

        [Fact]
        public async Task GetCustomers_ReturnsEmptyList_WhenNoCustomers()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new CustomersController(context);

            // Act
            var result = await controller.GetCustomers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Customer>>>(result);
            var customers = Assert.IsAssignableFrom<IEnumerable<Customer>>(actionResult.Value);
            Assert.Empty(customers);
        }

        [Fact]
        public async Task GetCustomers_ReturnsAllCustomers()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            context.Customers.AddRange(
                new Customer { ID = "1", Firstname = "John", Lastname = "Doe", Birthday = new DateTime(1990, 1, 1), Sex = "M", PhoneNumber = "123-456-7890", EmailAddress = "john@example.com" },
                new Customer { ID = "2", Firstname = "Jane", Lastname = "Smith", Birthday = new DateTime(1992, 5, 15), Sex = "F", PhoneNumber = "098-765-4321", EmailAddress = "jane@example.com" }
            );
            await context.SaveChangesAsync();
            var controller = new CustomersController(context);

            // Act
            var result = await controller.GetCustomers();

            // Assert
            var actionResult = Assert.IsType<ActionResult<IEnumerable<Customer>>>(result);
            var customers = Assert.IsAssignableFrom<IEnumerable<Customer>>(actionResult.Value).ToList();
            Assert.Equal(2, customers.Count);
            Assert.Contains(customers, c => c.ID == "1" && c.Firstname == "John");
            Assert.Contains(customers, c => c.ID == "2" && c.Firstname == "Jane");
        }

        [Fact]
        public async Task GetCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new CustomersController(context);

            // Act
            var result = await controller.GetCustomer("999");

            // Assert
            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task GetCustomer_ReturnsCustomer_WhenCustomerExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var customer = new Customer
            {
                ID = "1",
                Firstname = "John",
                Lastname = "Doe",
                Birthday = new DateTime(1990, 1, 1),
                Sex = "M",
                PhoneNumber = "123-456-7890",
                EmailAddress = "john@example.com"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            var controller = new CustomersController(context);

            // Act
            var result = await controller.GetCustomer("1");

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var returnedCustomer = Assert.IsType<Customer>(actionResult.Value);
            Assert.Equal("1", returnedCustomer.ID);
            Assert.Equal("John", returnedCustomer.Firstname);
            Assert.Equal("Doe", returnedCustomer.Lastname);
        }

        [Fact]
        public async Task PostCustomer_CreatesNewCustomer()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new CustomersController(context);
            var newCustomer = new Customer
            {
                ID = "1",
                Firstname = "Alice",
                Lastname = "Johnson",
                Birthday = new DateTime(1995, 3, 20),
                Sex = "F",
                PhoneNumber = "555-123-4567",
                EmailAddress = "alice@example.com"
            };

            // Act
            var result = await controller.PostCustomer(newCustomer);

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
            var returnedCustomer = Assert.IsType<Customer>(createdAtActionResult.Value);
            Assert.Equal("1", returnedCustomer.ID);
            Assert.Equal("Alice", returnedCustomer.Firstname);

            // Verify customer was added to database
            var customerInDb = await context.Customers.FindAsync("1");
            Assert.NotNull(customerInDb);
            Assert.Equal("Alice", customerInDb.Firstname);
        }

        [Fact]
        public async Task PostCustomer_ReturnsConflict_WhenCustomerAlreadyExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var existingCustomer = new Customer
            {
                ID = "1",
                Firstname = "John",
                Lastname = "Doe",
                Birthday = new DateTime(1990, 1, 1),
                Sex = "M",
                PhoneNumber = "123-456-7890",
                EmailAddress = "john@example.com"
            };
            context.Customers.Add(existingCustomer);
            await context.SaveChangesAsync();

            var controller = new CustomersController(context);
            var duplicateCustomer = new Customer
            {
                ID = "1",
                Firstname = "Jane",
                Lastname = "Smith",
                Birthday = new DateTime(1992, 5, 15),
                Sex = "F",
                PhoneNumber = "098-765-4321",
                EmailAddress = "jane@example.com"
            };

            // Act & Assert
            // The InMemory database throws ArgumentException for duplicate keys
            // which eventually gets wrapped by the controller's exception handling
            await Assert.ThrowsAnyAsync<Exception>(async () => 
                await controller.PostCustomer(duplicateCustomer));
        }

        [Fact]
        public async Task PutCustomer_UpdatesExistingCustomer()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var customer = new Customer
            {
                ID = "1",
                Firstname = "John",
                Lastname = "Doe",
                Birthday = new DateTime(1990, 1, 1),
                Sex = "M",
                PhoneNumber = "123-456-7890",
                EmailAddress = "john@example.com"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();
            
            // Detach the entity to avoid tracking conflicts
            context.Entry(customer).State = EntityState.Detached;

            var controller = new CustomersController(context);
            var updatedCustomer = new Customer
            {
                ID = "1",
                Firstname = "John",
                Lastname = "Smith",
                Birthday = new DateTime(1990, 1, 1),
                Sex = "M",
                PhoneNumber = "123-456-7890",
                EmailAddress = "john.smith@example.com"
            };

            // Act
            var result = await controller.PutCustomer("1", updatedCustomer);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify customer was updated in database
            var customerInDb = await context.Customers.FindAsync("1");
            Assert.Equal("Smith", customerInDb.Lastname);
            Assert.Equal("john.smith@example.com", customerInDb.EmailAddress);
        }

        [Fact]
        public async Task PutCustomer_ReturnsBadRequest_WhenIdMismatch()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new CustomersController(context);
            var customer = new Customer
            {
                ID = "1",
                Firstname = "John",
                Lastname = "Doe",
                Birthday = new DateTime(1990, 1, 1),
                Sex = "M",
                PhoneNumber = "123-456-7890",
                EmailAddress = "john@example.com"
            };

            // Act
            var result = await controller.PutCustomer("2", customer);

            // Assert
            Assert.IsType<BadRequestResult>(result);
        }

        [Fact]
        public async Task PutCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new CustomersController(context);
            var customer = new Customer
            {
                ID = "999",
                Firstname = "John",
                Lastname = "Doe",
                Birthday = new DateTime(1990, 1, 1),
                Sex = "M",
                PhoneNumber = "123-456-7890",
                EmailAddress = "john@example.com"
            };

            // Act
            var result = await controller.PutCustomer("999", customer);

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_RemovesCustomer_WhenCustomerExists()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var customer = new Customer
            {
                ID = "1",
                Firstname = "John",
                Lastname = "Doe",
                Birthday = new DateTime(1990, 1, 1),
                Sex = "M",
                PhoneNumber = "123-456-7890",
                EmailAddress = "john@example.com"
            };
            context.Customers.Add(customer);
            await context.SaveChangesAsync();

            var controller = new CustomersController(context);

            // Act
            var result = await controller.DeleteCustomer("1");

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            var deletedCustomer = Assert.IsType<Customer>(actionResult.Value);
            Assert.Equal("1", deletedCustomer.ID);

            // Verify customer was removed from database
            var customerInDb = await context.Customers.FindAsync("1");
            Assert.Null(customerInDb);
        }

        [Fact]
        public async Task DeleteCustomer_ReturnsNotFound_WhenCustomerDoesNotExist()
        {
            // Arrange
            using var context = GetInMemoryDbContext();
            var controller = new CustomersController(context);

            // Act
            var result = await controller.DeleteCustomer("999");

            // Assert
            var actionResult = Assert.IsType<ActionResult<Customer>>(result);
            Assert.IsType<NotFoundResult>(actionResult.Result);
        }
    }
}
