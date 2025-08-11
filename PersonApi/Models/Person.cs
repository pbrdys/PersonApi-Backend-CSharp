namespace PersonApi.Models
{
    /// <summary>
    /// Datenmodell für eine Person.
    /// </summary>
    public class Person
    {
        /// <summary>
        /// Eindeutige ID der Person.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Vorname der Person.
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// Nachname der Person.
        /// </summary>
        public string? Lastname { get; set; }

        /// <summary>
        /// Postleitzahl der Person.
        /// </summary>
        public string? Zipcode { get; set; }

        /// <summary>
        /// Wohnort der Person.
        /// </summary>
        public string? City { get; set; }

        /// <summary>
        /// Lieblingsfarbe der Person (z.B. als String).
        /// </summary>
        public string? Color { get; set; }
    }
}
