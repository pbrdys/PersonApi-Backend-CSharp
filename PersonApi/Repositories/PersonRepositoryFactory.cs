using PersonApi.Models;

namespace PersonApi.Repositories
{
    /// <summary>
    /// Factory, die basierend auf der Konfiguration CSV- oder DB-Repository zurückliefert.
    /// </summary>
    public class PersonRepositoryFactory : IPersonRepositoryFactory
    {
        private readonly CsvPersonRepository _csvRepo;
        private readonly DbPersonRepository _dbRepo;

        /// <summary>
        /// Initialisiert eine neue Instanz der <see cref="PersonRepositoryFactory"/>.
        /// </summary>
        /// <param name="csvRepo">CSV-Repository.</param>
        /// <param name="dbRepo">Datenbank-Repository.</param>
        public PersonRepositoryFactory(CsvPersonRepository csvRepo, DbPersonRepository dbRepo)
        {
            _csvRepo = csvRepo;
            _dbRepo = dbRepo;
        }

        /// <summary>
        /// Erzeugt ein Repository basierend auf dem angegebenen Datentyp.
        /// </summary>
        /// <param name="dataSourceType">Typ der Datenquelle.</param>
        /// <returns>Passendes Repository.</returns>
        public IPersonRepository CreateRepository(DataSourceType dataSourceType)
        {
            return dataSourceType switch
            {
                DataSourceType.Csv => _csvRepo,
                DataSourceType.Database => _dbRepo,
                _ => throw new ArgumentOutOfRangeException(nameof(dataSourceType), "Unsupported data source")
            };
        }
    }
}
