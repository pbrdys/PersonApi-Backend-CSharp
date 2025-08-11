using Microsoft.EntityFrameworkCore;
using PersonApi.Data;
using PersonApi.Models;
using PersonApi.Repositories;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PersonApi.Test
{
    public class DbPersonRepositoryTest : IDisposable
    {
        private readonly PersonDbContext _dbContext;
        private readonly DbPersonRepository _repo;

        public DbPersonRepositoryTest()
        {
            // InMemory DB konfigurieren
            var options = new DbContextOptionsBuilder<PersonDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // eindeutiger DB-Name für Isolation
                .Options;

            _dbContext = new PersonDbContext(options);
            _repo = new DbPersonRepository(_dbContext);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        private async Task SeedDataAsync()
        {
            _dbContext.Persons.AddRange(
                new Person { Id = 1, Lastname = "Müller", Name = "Hans", Zipcode = "12345", City = "Berlin", Color = "blau" },
                new Person { Id = 2, Lastname = "Schmidt", Name = "Anna", Zipcode = "54321", City = "Hamburg", Color = "grün" }
            );
            await _dbContext.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPersons()
        {
            await SeedDataAsync();

            var persons = await _repo.GetAllAsync();

            Assert.Equal(2, persons.Count());
            Assert.Contains(persons, p => p.Name == "Hans" && p.Lastname == "Müller");
            Assert.Contains(persons, p => p.Name == "Anna" && p.Lastname == "Schmidt");
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenNoPersons()
        {
            var persons = await _repo.GetAllAsync();

            Assert.NotNull(persons);
            Assert.Empty(persons);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenExists()
        {
            await SeedDataAsync();

            var person = await _repo.GetByIdAsync(1);

            Assert.NotNull(person);
            Assert.Equal("Hans", person.Name);
            Assert.Equal("Müller", person.Lastname);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenNotExists()
        {
            await SeedDataAsync();

            var person = await _repo.GetByIdAsync(9999);

            Assert.Null(person);
        }

        [Fact]
        public async Task GetByColorAsync_ShouldReturnPersons_WhenColorExists()
        {
            await SeedDataAsync();

            var blauPersons = await _repo.GetByColorAsync("blau");

            Assert.Single(blauPersons);
            Assert.All(blauPersons, p => Assert.Equal("blau", p.Color));
        }

        [Fact]
        public async Task GetByColorAsync_ShouldReturnEmptyList_WhenColorNotExists()
        {
            await SeedDataAsync();

            var persons = await _repo.GetByColorAsync("pink");

            Assert.NotNull(persons);
            Assert.Empty(persons);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPersonAndAssignId()
        {
            var newPerson = new Person
            {
                Name = "Max",
                Lastname = "Mustermann",
                Zipcode = "10115",
                City = "Berlin",
                Color = "violett"
            };

            var added = await _repo.AddAsync(newPerson);

            Assert.NotNull(added);
            Assert.True(added.Id > 0); // EF Core generiert Id beim Speichern
            Assert.Equal(newPerson.Name, added.Name);

            var all = await _repo.GetAllAsync();
            Assert.Contains(all, p => p.Name == "Max" && p.Lastname == "Mustermann");
        }

    }
}
