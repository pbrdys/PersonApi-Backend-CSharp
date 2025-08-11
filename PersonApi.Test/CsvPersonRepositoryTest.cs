using Microsoft.Extensions.Options;
using PersonApi.Models;
using PersonApi.Repositories;
using PersonApi.Services;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace PersonApi.Test
{
    public class CsvPersonRepositoryTest : IDisposable
    {
        private readonly string _dataFolder;
        private readonly string _csvFilePath;
        private readonly IOptions<ColorOptions> _options;

        public CsvPersonRepositoryTest()
        {
            // Ordner und Pfad für die Test-CSV
            _dataFolder = Path.Combine(Directory.GetCurrentDirectory(), "Data");
            if (!Directory.Exists(_dataFolder))
                Directory.CreateDirectory(_dataFolder);

            _csvFilePath = Path.Combine(_dataFolder, "sample-input.csv");

            var colorOptions = new ColorOptions
            {
                { 1, "blau" },
                { 2, "grün" },
                { 3, "violett" }
            };
            _options = Options.Create(colorOptions);
        }

        private CsvPersonRepository CreateRepository()
        {
            return new CsvPersonRepository(_options);
        }

        private void WriteCsv(string content)
        {
            File.WriteAllText(_csvFilePath, content, Encoding.UTF8);
        }

        public void Dispose()
        {
            // Nach Test: Datei löschen, falls noch vorhanden
            if (File.Exists(_csvFilePath))
                File.Delete(_csvFilePath);
        }

        [Fact]
        public async Task GetAllPersons_ShouldReturnAllPersons()
        {
            // Arrange
            var csv =
                    @"Müller, Hans, 12345 Berlin, 1
                    Schmidt, Anna, 54321 Hamburg, 2";
            WriteCsv(csv);

            var repo = CreateRepository();

            // Act
            var persons = await repo.GetAllAsync();

            // Assert
            Assert.Equal(2, persons.Count());
            Assert.Contains(persons, p => p.Name == "Hans" && p.Lastname == "Müller" && p.Zipcode == "12345" && p.City == "Berlin" && p.Color == "blau");
            Assert.Contains(persons, p => p.Name == "Anna" && p.Lastname == "Schmidt" && p.Zipcode == "54321" && p.City == "Hamburg" && p.Color == "grün");
        }

        [Fact]
        public async Task GetAllPersons_ShouldReturnEmptyList_WhenNoPersons()
        {
            // Arrange
            WriteCsv("");

            var repo = CreateRepository();

            // Act
            var persons = await repo.GetAllAsync();

            // Assert
            Assert.NotNull(persons);
            Assert.Empty(persons);
        }

        [Fact]
        public async Task GetById_ShouldReturnPerson_WhenExists()
        {
            // Arrange
            var csv =
                    @"Müller, Hans, 12345 Berlin, 1
                    Schmidt, Anna, 54321 Hamburg, 2";
            WriteCsv(csv);

            var repo = CreateRepository();

            var all = await repo.GetAllAsync();
            var person = all.First();

            // Act
            var result = await repo.GetByIdAsync(person.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(person.Id, result.Id);
            Assert.Equal(person.Name, result.Name);
        }

        [Fact]
        public async Task GetById_ShouldReturnNull_WhenNotExists()
        {
            // Arrange
            WriteCsv("Müller, Hans, 12345 Berlin, 1");

            var repo = CreateRepository();

            // Act
            var result = await repo.GetByIdAsync(9999);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetByColor_ShouldReturnPersons_WhenColorExists()
        {
            // Arrange
            var csv =
                    @"Müller, Hans, 12345 Berlin, 1
                    Schmidt, Anna, 54321 Hamburg, 2
                    Fischer, Max, 10115 Berlin, 1";
            WriteCsv(csv);

            var repo = CreateRepository();

            // Act
            var blauPersons = await repo.GetByColorAsync("blau");

            // Assert
            Assert.Equal(2, blauPersons.Count());
            Assert.All(blauPersons, p => Assert.Equal("blau", p.Color));
        }

        [Fact]
        public async Task GetByColor_ShouldReturnEmptyList_WhenColorNotExists()
        {
            // Arrange
            WriteCsv("Müller, Hans, 12345 Berlin, 1");

            var repo = CreateRepository();

            // Act
            var result = await repo.GetByColorAsync("pink");

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task AddAsync_ShouldAddPersonAndAssignId()
        {
            // Arrange
            var csv =
                    @"Müller, Hans, 12345 Berlin, 1
                    Schmidt, Anna, 54321 Hamburg, 2";
            WriteCsv(csv);

            var repo = CreateRepository();

            var newPerson = new Person
            {
                Name = "Max",
                Lastname = "Mustermann",
                Zipcode = "10115",
                City = "Berlin",
                Color = "violett"
            };

            // Act
            var addedPerson = await repo.AddAsync(newPerson);

            // Assert
            Assert.NotNull(addedPerson);
            Assert.True(addedPerson.Id > 0);
            Assert.Equal(newPerson.Name, addedPerson.Name);
            Assert.Equal(newPerson.Lastname, addedPerson.Lastname);
            Assert.Equal(newPerson.Zipcode, addedPerson.Zipcode);
            Assert.Equal(newPerson.City, addedPerson.City);
            Assert.Equal(newPerson.Color, addedPerson.Color);

            // Verify person is now returned by GetAllAsync
            var allPersons = await repo.GetAllAsync();
            Assert.Contains(allPersons, p => p.Id == addedPerson.Id && p.Name == "Max" && p.Lastname == "Mustermann");
        }

        [Fact]
        public async Task AddAsync_ShouldAppendLineToCsvFile()
        {
            // Arrange
            var csv =
                    @"Müller, Hans, 12345 Berlin, 1
                    Schmidt, Anna, 54321 Hamburg, 2";
            WriteCsv(csv);

            var repo = CreateRepository();

            var newPerson = new Person
            {
                Name = "Lisa",
                Lastname = "Meier",
                Zipcode = "20095",
                City = "Hamburg",
                Color = "grün"
            };

            // Act
            var addedPerson = await repo.AddAsync(newPerson);

            // Assert: CSV file contains new line with correct format
            var lines = File.ReadAllLines(_csvFilePath);

            Assert.Equal(3, lines.Length);

            // Die letzte Zeile sollte der neuen Person entsprechen
            var expectedColorId = _options.Value.First(kvp => kvp.Value == newPerson.Color).Key;
            var expectedLine = $"{newPerson.Lastname}, {newPerson.Name}, {newPerson.Zipcode} {newPerson.City}, {expectedColorId}";

            Assert.Equal(expectedLine, lines.Last());
        }

        [Fact(Skip = "In Anforderung nicht angegeben: Ich gehe davon aus, dass die Inhalte der CSV " +
            "vollständig sind, auch wenn die Daten nicht unbedingt in der gleichen Zeile stehen")]
        public async Task GetAllAsync_ShouldIgnoreEmptyOrIncompleteCsvLines()
        {
            // Arrange: Leere Zeilen und Zeilen mit weniger als 4 Teilen
            var csv =
                    @"Müller, Hans, 12345 Berlin, 1
                    ,,,
                    Schmidt, Anna, 54321 Hamburg"; // unvollständig (nur 3 Teile)
            WriteCsv(csv);

            var repo = CreateRepository();

            // Act
            var persons = await repo.GetAllAsync();

            // Assert: Nur valide Zeile wird geladen
            Assert.Single(persons);
            var person = persons.First();
            Assert.Equal("Hans", person.Name);
            Assert.Equal("Müller", person.Lastname);
        }

        [Fact]
        public void Constructor_ShouldThrowArgumentNullException_WhenColorOptionsIsNull()
        {
            // Arrange
            IOptions<ColorOptions> nullOptions = null!;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => new CsvPersonRepository(nullOptions));
        }
    }
}
