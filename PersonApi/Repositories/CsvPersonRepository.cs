using Microsoft.Extensions.Options;
using PersonApi.Models;
using PersonApi.Services;
using System.Globalization;

namespace PersonApi.Repositories
{
    /// <summary>
    /// Repository zum Lesen und Schreiben von Personen aus einer CSV-Datei.
    /// </summary>
    public class CsvPersonRepository : IPersonRepository
    {
        private readonly string _filePath;
        private readonly Dictionary<int, string> _colorMap;
        private List<Person>? _cache; // Cache für CSV-Inhalt
        private readonly object _lock = new();

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="CsvPersonRepository"/>.
        /// </summary>
        /// <param name="colorOptions">Farb-Mapping aus Konfiguration.</param>
        public CsvPersonRepository(IOptions<ColorOptions> colorOptions)
        {
            _filePath = Path.Combine(Directory.GetCurrentDirectory(), "Data", "sample-input.csv");
            // Sicherstellen, dass colorOptions nicht null ist
            _colorMap = colorOptions?.Value ?? throw new System.ArgumentNullException(nameof(colorOptions));
        }

        /// <summary>
        /// Gibt alle Personen aus der CSV zurück.
        /// </summary>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Alle Personen.</returns>
        public Task<IEnumerable<Person>> GetAllAsync(CancellationToken ct = default)
        {
            EnsureLoaded();
            return Task.FromResult(_cache!.AsEnumerable());
        }

        /// <summary>
        /// Liefert eine Person anhand der ID.
        /// </summary>
        /// <param name="id">ID der Person.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Gefundene Person oder null.</returns>
        public Task<Person?> GetByIdAsync(int id, CancellationToken ct = default)
        {
            EnsureLoaded();
            return Task.FromResult(_cache!.FirstOrDefault(p => p.Id == id));
        }

        /// <summary>
        /// Liefert alle Personen mit der angegebenen Farbe.
        /// </summary>
        /// <param name="color">Farbe als Filter.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Gefilterte Personen.</returns>
        public Task<IEnumerable<Person>> GetByColorAsync(string color, CancellationToken ct = default)
        {
            EnsureLoaded();
            return Task.FromResult(_cache!.Where(p => string.Equals(p.Color, color, StringComparison.OrdinalIgnoreCase)));
        }

        /// <summary>
        /// Fügt eine neue Person hinzu und speichert sie in der CSV.
        /// </summary>
        /// <param name="person">Neue Person.</param>
        /// <param name="ct">Cancellation Token.</param>
        /// <returns>Die hinzugefügte Person mit ID.</returns>
        public Task<Person> AddAsync(Person person, CancellationToken ct = default)
        {
            EnsureLoaded();
            if (_cache == null)
                throw new InvalidOperationException("Cache konnte nicht geladen werden.");

            person.Id = _cache!.Any() ? _cache.Max(p => p.Id) + 1 : 1;
            _cache.Add(person);

            // Neuen Eintrag in CSV-Datei anhängen
            var colorId = _colorMap.FirstOrDefault(c => c.Value == person.Color).Key;
            var line = $"{person.Lastname}, {person.Name}, {person.Zipcode} {person.City}, {colorId}";
            File.AppendAllText(_filePath, Environment.NewLine + line);

            return Task.FromResult(person);
        }

        // CSV nur einmal einlesen
        private void EnsureLoaded()
        {
            if (_cache != null) return;
            lock (_lock)
            {
                if (_cache != null) return;
                _cache = ParseCsv(File.ReadAllLines(_filePath));
            }
        }

        // CSV-Zeilen parsen
        private List<Person> ParseCsv(string[] rawLines)
        {
            var result = new List<Person>();
            int lineCounter = 0;

            foreach (var raw in rawLines)
            {
                var parts = raw.Replace("\uFEFF", "").Split(',', 4);
                if (parts.Length < 4) continue;

                var lastname = parts[0]?.Trim();
                var firstname = parts[1]?.Trim();
                var (zipcode, city) = ParseZipCity(parts[2]?.Trim());
                var color = ParseColor(parts[3]?.Trim());

                lineCounter++;
                result.Add(new Person
                {
                    Id = lineCounter,
                    Lastname = lastname,
                    Name = firstname,
                    Zipcode = zipcode,
                    City = city,
                    Color = color
                });
            }
            return result;
        }

        // Postleitzahl + Ort aus String extrahieren
        private static (string? zipcode, string? city) ParseZipCity(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return (null, null);
            var match = System.Text.RegularExpressions.Regex.Match(raw, "^(\\d{5})\\s+(.*)$");
            if (match.Success)
                return (match.Groups[1].Value, match.Groups[2].Value);
            return (null, raw);
        }

        // Farbcode-ID in Namen umwandeln
        private string? ParseColor(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            if (int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
                return _colorMap.TryGetValue(id, out var color) ? color : null;
            return raw;
        }
    }
}
