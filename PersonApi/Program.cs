using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using PersonApi.Data;
using PersonApi.Models;
using PersonApi.Repositories;
using PersonApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Konfiguration lesen
bool importOnStartup = builder.Configuration.GetValue<bool>("ImportOnStartup");

// Farb-Mapping aus Config binden
builder.Services.Configure<ColorOptions>(builder.Configuration.GetSection("ColorMapping"));

// DbContext konfigurieren (nur bei UseDatabase relevant)
builder.Services.AddDbContext<PersonDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories und Factory registrieren
builder.Services.AddSingleton<CsvPersonRepository>(); // Singleton, da CSV nur einmal gelesen wird
builder.Services.AddScoped<DbPersonRepository>();
builder.Services.AddScoped<IPersonRepositoryFactory, PersonRepositoryFactory>();

// IPersonRepository dynamisch auswählen
var dataSourceTypeString = builder.Configuration.GetValue<string>("DataSourceType");
if (string.IsNullOrWhiteSpace(dataSourceTypeString))
    throw new InvalidOperationException("Die Konfiguration 'DataSourceType' ist nicht gesetzt oder leer.");
var dataSourceType = Enum.Parse<DataSourceType>(dataSourceTypeString, ignoreCase: true);


builder.Services.AddScoped(sp =>
{
    var factory = sp.GetRequiredService<IPersonRepositoryFactory>();
    return factory.CreateRepository(dataSourceType);
});

// CSV->DB Importer registrieren
builder.Services.AddScoped<CsvToDbImporter>();

// Standard-API-Setup
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger nur in Development anzeigen
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Optionalen Import beim Start durchführen
if (importOnStartup)
{
    using var scope = app.Services.CreateScope();
    var importer = scope.ServiceProvider.GetRequiredService<CsvToDbImporter>();
    await importer.ImportAsync();
}

app.MapControllers();
app.Run();
