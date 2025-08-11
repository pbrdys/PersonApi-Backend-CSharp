using Microsoft.EntityFrameworkCore;
using PersonApi.Data;
using PersonApi.Models;

namespace PersonApi.Repositories
{
    /// <summary>
    /// Repository zur Verwaltung von Personen in der Datenbank.
    /// </summary>
    public class DbPersonRepository : IPersonRepository
    {
        private readonly PersonDbContext _db;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="DbPersonRepository"/>.
        /// </summary>
        /// <param name="db">Datenbank-Kontext.</param>
        public DbPersonRepository(PersonDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Liefert alle Personen aus der Datenbank.
        /// </summary>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Alle Personen.</returns>
        public async Task<IEnumerable<Person>> GetAllAsync(CancellationToken ct = default)
        {
            // Holt alle Personen aus der Datenbank (AsNoTracking für bessere Performance, da keine Änderungen geplant sind)
            return await _db.Persons.AsNoTracking().ToListAsync(ct);
        }

        /// <summary>
        /// Liefert eine Person anhand der ID aus der Datenbank.
        /// </summary>
        /// <param name="id">ID der Person.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Gefundene Person oder null.</returns>
        public async Task<Person?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            // Sucht in der Datenbank nach einer Person mit der angegebenen ID
            // Gibt null zurück, wenn keine Person gefunden wurde
            return await _db.Persons.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        }

        /// <summary>
        /// Liefert alle Personen mit der angegebenen Farbe aus der Datenbank.
        /// </summary>
        /// <param name="color">Farbe als Filter.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Gefilterte Personen.</returns>
        public async Task<IEnumerable<Person>> GetByColorAsync(string color, CancellationToken ct = default)
        {
            // Wenn keine Farbe angegeben wurde, direkt leere Liste zurückgeben
            if (string.IsNullOrWhiteSpace(color)) return Enumerable.Empty<Person>();

            // Holt alle Personen mit passender Farbe (Case-Insensitive)
            return await _db.Persons.AsNoTracking()
                .Where(p => p.Color != null && p.Color.ToLower() == color.ToLower())
                .ToListAsync(ct);
        }

        /// <summary>
        /// Fügt eine neue Person in die Datenbank ein.
        /// </summary>
        /// <param name="person">Neue Person.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Die hinzugefügte Person.</returns>
        public async Task<Person> AddAsync(Person person, CancellationToken ct = default)
        {
            // Fügt eine neue Person in die Datenbank ein und speichert die Änderungen
            _db.Persons.Add(person);
            await _db.SaveChangesAsync(ct);
            return person;
        }
    }
}
