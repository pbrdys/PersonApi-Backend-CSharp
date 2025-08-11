using Microsoft.AspNetCore.Mvc;
using PersonApi.Models;
using PersonApi.Repositories;

namespace PersonApi.Controllers
{
    /// <summary>
    /// API-Controller für die Verwaltung von Personen.
    /// Bietet Endpunkte zum Abrufen, Filtern und Erstellen von Personen.
    /// </summary>
    [ApiController]
    [Route("persons")] // Basis-Route für alle Endpunkte
    public class PersonsController : ControllerBase
    {
        private readonly IPersonRepository _repo;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="PersonsController"/> mit dem angegebenen Repository.
        /// </summary>
        /// <param name="repo">Das Repository für Personen.</param>
        public PersonsController(IPersonRepository repo)
        {
            _repo = repo;
        }

        /// <summary>
        /// Gibt alle Personen zurück.
        /// </summary>
        /// <returns>Eine Liste aller Personen.</returns>
        [HttpGet]
        public async Task<IEnumerable<Person>> GetAll()
        {
            return await _repo.GetAllAsync();
        }

        /// <summary>
        /// Ruft eine einzelne Person anhand der angegebenen ID ab.
        /// </summary>
        /// <param name="id">Die ID der gesuchten Person.</param>
        /// <returns>
        /// 200 OK mit der Person, wenn gefunden; 
        /// 404 Not Found, wenn keine Person mit der ID existiert.
        /// </returns>
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var p = await _repo.GetByIdAsync(id);
            if (p == null) return NotFound(); // Kein Treffer -> 404
            return Ok(p); // Treffer -> 200 mit Personendaten
        }

        /// <summary>
        /// Ruft alle Personen mit der angegebenen Farbe ab.
        /// </summary>
        /// <param name="color">Die Farbe, nach der gefiltert werden soll.</param>
        /// <returns>Eine Liste von Personen mit der angegebenen Farbe (kann leer sein).</returns>
        [HttpGet("color/{color}")]
        public async Task<IEnumerable<Person>> GetByColor(string color)
        {
            return await _repo.GetByColorAsync(color);
        }

        /// <summary>
        /// Fügt eine neue Person hinzu.
        /// </summary>
        /// <param name="newPerson">Das Objekt der hinzuzufügenden Person.</param>
        /// <returns>
        /// 201 Created mit der neuen Person, wenn erfolgreich;
        /// 400 Bad Request, wenn die übergebenen Daten ungültig sind.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> AddPerson([FromBody] Person newPerson)
        {
            if (newPerson == null) 
                return BadRequest(new { error = "Person data must be provided." });

            if (string.IsNullOrWhiteSpace(newPerson.Name) || string.IsNullOrWhiteSpace(newPerson.Lastname))
                return BadRequest(new { error = "Name and Lastname are required." });

            var created = await _repo.AddAsync(newPerson);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created); // Erfolgreich -> 201
        }
    }
}
