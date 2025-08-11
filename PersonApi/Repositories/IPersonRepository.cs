using PersonApi.Models;

namespace PersonApi.Repositories
{
    /// <summary>
    /// Interface für Person-Repository mit typischen CRUD-Operationen.
    /// </summary>
    public interface IPersonRepository
    {
        /// <summary>
        /// Liefert alle Personen.
        /// </summary>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Auflistung aller Personen.</returns>
        Task<IEnumerable<Person>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Liefert eine Person anhand der ID.
        /// </summary>
        /// <param name="id">ID der Person.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Gefundene Person oder null.</returns>
        Task<Person?> GetByIdAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Liefert alle Personen anhand der angegebenen Farbe.
        /// </summary>
        /// <param name="color">Farbe als Filter.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Gefilterte Personenliste.</returns>
        Task<IEnumerable<Person>> GetByColorAsync(string color, CancellationToken ct = default);

        /// <summary>
        /// Fügt eine neue Person hinzu.
        /// </summary>
        /// <param name="person">Person, die hinzugefügt werden soll.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Die hinzugefügte Person mit ggf. gesetzter ID.</returns>
        Task<Person> AddAsync(Person person, CancellationToken ct = default);
    }
}
