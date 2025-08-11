using PersonApi.Models;

namespace PersonApi.Repositories
{
    /// <summary>
    /// Interface für eine Factory, die das passende <see cref="IPersonRepository"/> erzeugt.
    /// </summary>
    public interface IPersonRepositoryFactory
    {
        /// <summary>
        /// Erzeugt ein Repository basierend auf dem angegebenen Datentyp.
        /// </summary>
        /// <param name="dataSourceType">Typ der Datenquelle.</param>
        /// <returns>Passendes Repository.</returns>
        IPersonRepository CreateRepository(DataSourceType dataSourceType);
    }
}
