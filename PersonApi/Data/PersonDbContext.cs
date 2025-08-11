using Microsoft.EntityFrameworkCore;
using PersonApi.Models;

namespace PersonApi.Data
{
    /// <summary>
    /// EF Core DbContext für den Zugriff auf Person-Daten.
    /// </summary>
    public class PersonDbContext : DbContext
    {
        /// <summary>
        /// DbSet der Personen.
        /// </summary>
        public DbSet<Person> Persons { get; set; } = null!;

        /// <summary>
        /// Initialisiert eine neue Instanz des <see cref="PersonDbContext"/>.
        /// </summary>
        /// <param name="options">Konfigurationsoptionen für den DbContext.</param>
        public PersonDbContext(DbContextOptions<PersonDbContext> options) : base(options) { }

        /// <summary>
        /// Konfiguriert das Datenmodell.
        /// </summary>
        /// <param name="modelBuilder">Modell-Builder.</param>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Person>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Lastname).HasMaxLength(200);
                entity.Property(p => p.Name).HasMaxLength(200);
                entity.Property(p => p.Zipcode).HasMaxLength(20);
                entity.Property(p => p.City).HasMaxLength(200);
                entity.Property(p => p.Color).HasMaxLength(100);
            });
        }
    }
}
