using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Moq;
using PersonApi.Controllers;
using PersonApi.Models;
using PersonApi.Repositories;
using Xunit;

namespace PersonApi.Test
{
    public class PersonsControllerTest
    {
        private readonly Mock<IPersonRepository> _mockRepo;
        private readonly PersonsController _controller;

        public PersonsControllerTest()
        {
            _mockRepo = new Mock<IPersonRepository>();
            _controller = new PersonsController(_mockRepo.Object);
        }

        [Fact]
        public async Task GetAll_ShouldReturnAllPersons()
        {
            // Arrange
            var persons = new List<Person>
            {
                new Person { Id = 1, Name = "Hans", Lastname = "Müller" },
                new Person { Id = 2, Name = "Anna", Lastname = "Schmidt" }
            };
            _mockRepo.Setup(r => r.GetAllAsync(default)).ReturnsAsync(persons);

            // Act
            var result = await _controller.GetAll();

            // Assert
            Assert.Equal(persons.Count, result.Count());
            Assert.Contains(result, p => p.Name == "Hans");
            Assert.Contains(result, p => p.Name == "Anna");
        }

        [Fact]
        public async Task GetById_ShouldReturnOk_WhenPersonExists()
        {
            // Arrange
            var person = new Person { Id = 1, Name = "Hans", Lastname = "Müller" };
            _mockRepo.Setup(r => r.GetByIdAsync(1, default)).ReturnsAsync(person);

            // Act
            var actionResult = await _controller.GetById(1);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(actionResult);
            var returnedPerson = Assert.IsType<Person>(okResult.Value);
            Assert.Equal(person.Id, returnedPerson.Id);
        }

        [Fact]
        public async Task GetById_ShouldReturnNotFound_WhenPersonDoesNotExist()
        {
            // Arrange
            _mockRepo.Setup(r => r.GetByIdAsync(42, default)).ReturnsAsync((Person?)null);

            // Act
            var actionResult = await _controller.GetById(42);

            // Assert
            Assert.IsType<NotFoundResult>(actionResult);
        }

        [Fact]
        public async Task GetByColor_ShouldReturnPersonsWithColor()
        {
            // Arrange
            var color = "blau";
            var persons = new List<Person>
            {
                new Person { Id = 1, Color = "blau" },
                new Person { Id = 2, Color = "blau" },
                new Person { Id = 3, Color = "grün" },
            };
            _mockRepo.Setup(r => r.GetByColorAsync(color, default))
                    .ReturnsAsync(persons.Where(p => p.Color == color));

            // Act
            var result = await _controller.GetByColor(color);

            // Assert
            Assert.All(result, p => Assert.Equal("blau", p.Color));
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetByColor_ShouldReturnEmptyList_WhenColorNotExists()
        {
            // Arrange
            var color = "lila";
            var emptyList = new List<Person>();
            _mockRepo.Setup(r => r.GetByColorAsync(color, default)).ReturnsAsync(emptyList);

            // Act
            var result = await _controller.GetByColor(color);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddPerson_ShouldReturnCreatedAtAction_WhenPersonIsValid()
        {
            // Arrange
            var newPerson = new Person { Name = "Max", Lastname = "Mustermann" };
            var createdPerson = new Person { Id = 123, Name = "Max", Lastname = "Mustermann" };
            _mockRepo.Setup(r => r.AddAsync(newPerson, default)).ReturnsAsync(createdPerson);

            // Act
            var actionResult = await _controller.AddPerson(newPerson);

            // Assert
            var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult);
            Assert.Equal(nameof(_controller.GetById), createdAtResult.ActionName);
            Assert.Equal(createdPerson.Id, ((Person)createdAtResult.Value!).Id);
        }

        [Fact]
        public async Task AddPerson_ShouldReturnBadRequest_WhenPersonIsNull()
        {
            // Act
            var result = await _controller.AddPerson(null!);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Person data must be provided", badRequest.Value!.ToString()!);
        }

        [Theory]
        [InlineData(null, "Lastname")]
        [InlineData("Name", null)]
        [InlineData("", "Lastname")]
        [InlineData("Name", "")]
        [InlineData(" ", "Lastname")]
        [InlineData("Name", " ")]
        public async Task AddPerson_ShouldReturnBadRequest_WhenNameOrLastnameInvalid(string? name, string? lastname)
        {
            // Arrange
            var newPerson = new Person { Name = name, Lastname = lastname };

            // Act
            var result = await _controller.AddPerson(newPerson);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Name and Lastname are required", badRequest.Value!.ToString()!);
        }
    }
}
