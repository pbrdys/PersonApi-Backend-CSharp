using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonApi.Data;
using PersonApi.Models;
using PersonApi.Repositories;
using PersonApi.Services;
using System;
using Xunit;

namespace PersonApi.Test
{
    public class PersonRepositoryFactoryTest
    {
        private readonly CsvPersonRepository _csvRepo;
        private readonly DbPersonRepository _dbRepo;
        private readonly PersonRepositoryFactory _factory;

        public PersonRepositoryFactoryTest()
        {
            var optionsMap = new ColorOptions
            {
                { 1, "blau" },
                { 2, "grün" },
            };
            var colorOptions = Options.Create(optionsMap);
            _csvRepo = new CsvPersonRepository(colorOptions);


            // InMemory DB konfigurieren
            var dbOptions = new DbContextOptionsBuilder<PersonDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString()) // eindeutiger DB-Name für Isolation
                .Options;

            var dbContext = new PersonDbContext(dbOptions);
            _dbRepo = new DbPersonRepository(dbContext);

            _factory = new PersonRepositoryFactory(_csvRepo, _dbRepo);
        }

        [Fact]
        public void CreateRepository_ShouldReturnCsvRepository_WhenDataSourceTypeIsCsv()
        {
            // Act
            var repo = _factory.CreateRepository(DataSourceType.Csv);

            // Assert
            Assert.NotNull(repo);
            Assert.IsType<CsvPersonRepository>(repo);
            Assert.Equal(_csvRepo, repo);
        }

        [Fact]
        public void CreateRepository_ShouldReturnDbRepository_WhenDataSourceTypeIsDatabase()
        {
            // Act
            var repo = _factory.CreateRepository(DataSourceType.Database);

            // Assert
            Assert.NotNull(repo);
            Assert.IsType<DbPersonRepository>(repo);
            Assert.Equal(_dbRepo, repo);
        }

        [Fact]
        public void CreateRepository_ShouldThrowArgumentOutOfRangeException_WhenInvalidDataSourceType()
        {
            // Arrange
            var invalidDataSourceType = (DataSourceType)999;

            // Act & Assert
            var ex = Assert.Throws<ArgumentOutOfRangeException>(() => _factory.CreateRepository(invalidDataSourceType));
            Assert.Contains("Unsupported data source", ex.Message);
        }
    }
}
