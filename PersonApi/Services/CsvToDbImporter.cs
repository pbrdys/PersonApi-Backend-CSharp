using PersonApi.Data;
using PersonApi.Repositories;

namespace PersonApi.Services
{
    /// <summary>
    /// Importiert Personen-Daten aus einer CSV-Datei in die Datenbank.
    /// </summary>
    public class CsvToDbImporter
    {
        private readonly CsvPersonRepository _csvRepo;
        private readonly PersonDbContext _dbContext;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="CsvToDbImporter"/>.
        /// </summary>
        /// <param name="csvRepo">CSV-Repository zum Lesen der Daten.</param>
        /// <param name="dbContext">Datenbank-Kontext zum Speichern der Daten.</param>
        public CsvToDbImporter(CsvPersonRepository csvRepo, PersonDbContext dbContext)
        {
            _csvRepo = csvRepo;
            _dbContext = dbContext;
        }

        /// <summary>
        /// Führt den Import der Daten aus der CSV-Datei in die Datenbank asynchron durch.
        /// </summary>
        /// <returns>Ein Task, der den Import repräsentiert.</returns>
        public async Task ImportAsync()
        {
            var persons = await _csvRepo.GetAllAsync();

            // ACHTUNG: Löscht bestehende Daten!
            _dbContext.Persons.RemoveRange(_dbContext.Persons);
            await _dbContext.SaveChangesAsync();

            await _dbContext.Persons.AddRangeAsync(persons);
            await _dbContext.SaveChangesAsync();
        }
    }
}
