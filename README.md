# PersonApi

## Übersicht
**PersonApi** ist eine einfache REST-API zur Verwaltung von Personendaten. 
Die API unterst&uuml;tzt CRUD-Operationen (vor allem Lesen und Hinzuf&uuml;gen) und kann Daten flexibel aus 
zwei Quellen laden: einer CSV-Datei oder einer SQLite-Datenbank. 
Die Datenquelle l&auml;sst sich bequem per Konfiguration ausw&auml;hlen.

## Projektstruktur
PersonApi
- Controllers
  - PersonController.cs bietet Endpunkte für:
    - GET /persons (alle)
    - GET /persons/{id} (nach ID)
    - GET /persons/color/{color} (filtern nach Farbe)
    - POST /persons (neue Person hinzufügen)
- Data
  - PersonDbContext.cs ist der EF Core Kontext mit Constraints und Konfiguration.
  - sample-input.csv
- Migrations
  - 20250808071248_InitialCreate.cs
  - PersonDbContextModelSnapshot.cs
- Models
  - Person.cs
  - DataSourceType.cs legt Quelle der Datenherkunft fest (Csv, Db, ...).
- Repositories
  - CsvPersonRepository.cs lädt Personen aus Data/sample-input.csv und cached sie.
  - DbPersonRepository.cs greift direkt auf EF Core zu (SQLite).
  - IPersonRepository.cs ist das gemeinsame Interface für CSV- und DB-Implementierungen.
  - IPersonRepositoryFactory.cs
  - PersonRepositoryFactory.cs liefert je nach DataSourceType das passende Repository.
- Services
  - ColorOptions.cs kapselt Farbzuordnungen aus der appsettings.json.
  - CsvToDbImporter.cs kann CSV-Daten in die DB importieren (löscht dabei alle vorhandenen Einträge).
- appsettings.json
- persons.db
- Program.cs
    - Liest Konfiguration (DataSourceType, UseDatabase, ImportOnStartup).
    - Registriert CSV- oder DB-Repository über Factory.
    - Startet optional CSV->DB Import.
    - Bindet ColorMapping aus Config.
    - Swagger ist in Development aktiv.


## Architektur & Design

### Architekturstil
Das Projekt folgt einer **Schichtenarchitektur** mit klar getrennten Verantwortlichkeiten:

- **Models:** Datenstrukturen wie `Person` und `DataSourceType`.
- **Data Layer:** EF Core `DbContext` f&uuml;r Datenbankzugriff und CSV-Datei.
- **Repositories:** Abstraktion f&uuml;r den Datenzugriff (CSV, DB, ...)
- **Services:** Business-Logik (CSV -> DB Import, ColorMapping)
- **Controllers:** API-Endpunkte (HttpGet, HttpPost)
- **Dependency Injection:** Erm&ouml;glicht flexible und testbare Komponenten.

### Vorteile dieser Architektur
1. **Flexibilit&auml;t bei Datenquellen:** Einfacher Wechsel zwischen CSV, DB und anderen Datenquellen ohne Code&auml;nderung im Controller.
2. **Separation of Concerns:** Klare Aufgabenverteilung, z.B. CSV-Parsing nur im `CsvPersonRepository`.
3. **Erweiterbarkeit:** Neue Datenquellen (z.B. JSON, Remote-API) k&ouml;nnen leicht erg&auml;nzt werden.
4. **Testbarkeit:** Interfaces und DI erm&ouml;glichen einfache Mock-Implementierungen.


## Erkl&auml;rung der wichtigsten Komponenten

### Models
- **Person.cs:** Datenmodell f&uuml;r eine Person (Id, Name, Lastname, Zipcode, City, Color).
- **DataSourceType.cs:** Enum zur Auswahl der Datenquelle (`Csv` oder `Database`).

### Data
- **PersonDbContext.cs:** EF Core DbContext f&uuml;r SQLite mit `DbSet<Person>` und Modellkonfiguration.
- **sample-input.csv:** Daten-Basis

### Services
- **ColorOptions.cs:** Bindet Farb-IDs aus `appsettings.json` als Dictionary ein.
- **CsvToDbImporter.cs:** Importiert CSV-Daten in die DB beim Start (optional).

### Repositories
- **IPersonRepository.cs:** Gemeinsames Interface für Datenzugriffsmethoden (`GetAll`, `GetById`, `GetByColor`, `Add`).
- **CsvPersonRepository.cs:** Implementierung für CSV-Dateien, inklusive Caching und Farb-Mapping.
- **DbPersonRepository.cs:** Implementierung für SQLite-Datenbank mit EF Core.
- **PersonRepositoryFactory.cs:** Liefert je nach `DataSourceType` das passende Repository.
- **IPersonRepositoryFactory.cs:** Interface der Factory.

### Controllers
- **PersonsController.cs:** REST-API-Endpunkte:
  - `GET /persons` – Alle Personen abrufen
  - `GET /persons/{id}` – Person per ID abrufen
  - `GET /persons/color/{color}` – Personen nach Farbe filtern
  - `POST /persons` – Neue Person hinzufügen

### Program.cs
- Konfiguriert Services und DI, lädt Farb-Mapping, DbContext, Repository-Factory, CSV→DB Importer, API-Controller, Swagger.
- Optionaler CSV→DB Import beim Start über Konfiguration steuerbar.

---

## Konfigurationsdatei (appsettings.json)
- Steuerung von Logging, erlaubten Hosts.
- Auswahl der Datenquelle via `"DataSourceType": "Csv"` oder `"Database"`.
- Steuerung des CSV->DB Imports (`ImportOnStartup`).
- SQLite-Verbindungsstring.
- Farb-Mapping als Dictionary (ID zu Farbname).


| Schlüssel                             | Beschreibung                                                                                                                 | Beispiel                   |
| ------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- | -------------------------- |
| `DataSourceType`                      | Datenquelle auswählen: `"Csv"` (Datei) oder `"Database"` (SQLite)                                                            | `"Csv"`                    |
| `ImportOnStartup`                     | Wenn `true`, werden beim API-Start CSV-Daten in die DB importiert und DB-Daten überschrieben (Achtung: Datenverlust möglich) | `false`                    |
| `ConnectionStrings:DefaultConnection` | SQLite-Verbindungsstring für EF Core DB-Zugriff                                                                              | `"Data Source=persons.db"` |
| `ColorMapping`                        | Zuordnung von Farb-IDs zu Farbnamen, verwendet beim CSV-Import und Farb-Parsing                                              | `"1": "blau"`              |

---

## Nutzung
1. Datenquelle konfigurieren
W&auml;hle in appsettings.json den Parameter DataSourceType:
  - "Csv": Daten werden aus der CSV-Datei Data/sample-input.csv geladen. Änderungen werden direkt in der CSV-Datei gespeichert.
  - "Database": Daten werden aus der SQLite-Datenbank persons.db gelesen. Die DB kann mittels Migrationen aufgebaut werden.

2. (Optional) CSV -> DB Import aktivieren
M&ouml;chtest du beim Start automatisch die CSV-Daten in die Datenbank importieren (überschreibt vorhandene Daten), stelle ImportOnStartup auf true und setze DataSourceType auf "Database".
Dieser Vorgang wird &uuml;ber den CsvToDbImporter realisiert.
3. API starten
Starte die Anwendung, z.B. in Visual Studio oder via dotnet run.
Die API ist dann über Endpunkte wie GET /persons oder POST /persons erreichbar.

### API-Endpunkte 
| Methode | URL                      | Beschreibung                    | Body (POST)            |
| ------- | ------------------------ | ------------------------------- | ---------------------- |
| GET     | `/persons`               | Liste aller Personen abrufen    | -                      |
| GET     | `/persons/{id}`          | Einzelne Person nach ID abrufen | -                      |
| GET     | `/persons/color/{color}` | Personen nach Farbe filtern     | -                      |
| POST    | `/persons`               | Neue Person hinzufügen          | JSON mit Personendaten |


Beispiel POST-Body: 
```json 
{
  "name": "Max",
  "lastname": "Mustermann",
  "zipcode": "12345",
  "city": "Musterstadt",
  "color": "blau"
}
```

### CSV-Datenformat
Die CSV-Datei `sample-input.csv` liegt im Verzeichnis `/Data` und hat folgendes Format pro Zeile:
```json
Lastname, Name, Zipcode City, ColorId
```
Beispiel:
```json 
Müller, Hans, 67742 Lauterecken, 1
Petersen, Peter, 18439 Stralsund, 2
```
`ColorId` wird &uuml;ber die Farb-IDs aus `appsettings.json` auf Farbname aufgel&ouml;st.

## Technische Details & Hinweise
### Dependency Injection (DI) & Repository Pattern
- Über das Interface IPersonRepository werden Datenzugriffe abstrahiert.
- Die konkrete Implementierung (CsvPersonRepository oder DbPersonRepository) wird je nach DataSourceType ausgewählt.
- Die PersonRepositoryFactory verwaltet die Auswahl.

### CSV-Handling
- Die CSV-Datei wird beim Start eingelesen und im Speicher gecached.
- Änderungen (neue Personen) werden als neue Zeilen angehängt.
- Farben werden über die ID in der CSV mit den Farbnamen aus ColorMapping verknüpft.

### Datenbank
- SQLite als leichtgewichtige DB ohne Server benötigt keine Installation.
- EF Core verwaltet das Datenmodell und Migrationen.
- Datenbank und Migrationen sind im Projekt enthalten.
- persons.db wird automatisch angelegt, wenn nicht vorhanden.

### CSV -> DB Import
- Wird über den Service CsvToDbImporter gesteuert.
- Beim Import werden alle bestehenden DB-Daten gelöscht und durch CSV-Daten ersetzt.
- Import kann im Code oder per Konfiguration beim Start aktiviert werden.

### Farb-Mapping
- Farb-IDs aus CSV werden in Farbnamen übersetzt.
- Farb-Mapping ist im appsettings.json konfigurierbar und wird per Options-Muster gebunden (ColorOptions).


## Entwicklung & Setup
### Voraussetzungen
- .NET 8 SDK (LTS)
- Visual Studio 2022 oder höher / beliebiger Editor mit CLI

### Abhängigkeiten
- ASP.NET Core Web API
- Entity Framework Core mit SQLite Provider

### Setup Schritte
1. Repository klonen
2. NuGet-Pakete wiederherstellen:
```bash 
dotnet restore
```
3. Migrationen anwenden (nur bei DB-Nutzung):
```bash
dotnet ef database update
```
4. appsettings.json anpassen (Datenquelle, DB-Connection, Farb-Mapping)
5. API starten:
```bash
dotnet run
```

### Warum SQLite?

- **Einfachheit:** Keine Serverinstallation nötig.
- **Portabilität:** Datenbank liegt als einzelne Datei vor.
- **Performance:** Für kleine/mittlere Datenmengen performant.
- **Einfaches Setup:** Kein Admin-Aufwand.
- **Ideal für Prototyping/Demos.**

---

### SQLite Installation & Setup (für Visual Studio)

1. **NuGet-Pakete installieren:**
2. **Optional:** SQLite-Tools herunterladen für CLI oder DB-Browser:  
https://www.sqlite.org/download.html
3. **Verbindungsstring in `appsettings.json`:**
```json
"ConnectionStrings": {
  "DefaultConnection": "Data Source=persons.db"
}
```
4. **DbContext registrieren & Migrationen anlegen:**
```bash 
dotnet ef migrations add InitialCreate
dotnet ef database update
```


## FAQ & Troubleshooting
### Warum werden beim CSV→DB Import Daten gelöscht?
Damit die DB stets den CSV-Stand widerspiegelt, löscht der Import alle bestehenden Datensätze vor dem Einfügen.

### Wie kann ich eine neue Datenquelle hinzufügen?
1. Neues Repository implementieren (Interface IPersonRepository beachten)
2. Neue Option im Enum DataSourceType ergänzen
3. Anpassung der Factory PersonRepositoryFactory
4. Konfiguration in appsettings.json erweitern

### Wie kann ich Farben anpassen?
In appsettings.json unter ColorMapping die Farb-IDs und Namen ändern oder ergänzen.


## Wichtige Hinweise 
- Beim CSV→DB Import werden alle bestehenden Daten in der Datenbank gelöscht.
- Die CSV-Datei wird beim Zugriff auf das Repository einmal eingelesen und gecached.
- Farb-Mapping wird aus der Konfiguration geladen und im CSV-Repository verwendet.
- Die Repository-Auswahl erfolgt dynamisch über eine Factory anhand der Konfiguration.


# PersonApi.Test

Dieses Projekt enthält Unit-Tests für das `PersonApi`-Projekt. Die Tests sind mit xUnit umgesetzt und prüfen alle wesentlichen Funktionen der API, der Repository-Schichten sowie des CSV-Importers.

---

## Testabdeckung

### 1. **CsvPersonRepositoryTests**

- **GetAllAsync**  
  Prüft, ob alle Personen aus der CSV korrekt geladen werden.

- **GetByIdAsync**  
  Testet das Finden einer Person anhand der ID, inklusive Rückgabe von `null` bei nicht vorhandenem Eintrag.

- **GetByColorAsync**  
  Prüft die Filterung der Personenliste nach Farbe (Case-Insensitive).

- **AddAsync**  
  Testet das Hinzufügen einer neuen Person:
  - Neue ID wird korrekt vergeben.
  - Person wird im internen Cache und in der CSV-Datei gespeichert.

- **ParseCsv & Hilfsmethoden**  
  Validiert das korrekte Parsen der CSV-Datei, inklusive korrekter Extraktion von Postleitzahl, Ort und Farbzuordnung.

---

### 2. **DbPersonRepositoryTests**

- **GetAllAsync**  
  Holt alle Personen aus der Datenbank und prüft die Rückgabe.

- **GetByIdAsync**  
  Findet eine Person anhand der ID oder gibt `null` zurück, falls nicht vorhanden.

- **GetByColorAsync**  
  Filtert Personen nach Farbe, berücksichtigt Groß-/Kleinschreibung und leere Eingaben.

- **AddAsync**  
  Fügt eine Person in die Datenbank ein und speichert die Änderungen.

---

### 3. **PersonRepositoryFactoryTests**

- Prüft, dass abhängig vom `DataSourceType` das richtige Repository zurückgegeben wird.

- Validiert, dass bei ungültigem `DataSourceType` eine Exception geworfen wird.

---

### 4. **CsvToDbImporterTests**

- Testet den Import-Prozess:
  - Löschen bestehender Einträge in der DB.
  - Einfügen der CSV-Daten in die Datenbank.

---

### 5. **PersonsControllerTests**

- **GetAll**  
  Testet, ob alle Personen zurückgegeben werden.

- **GetById**  
  Prüft Rückgabe von `200 OK` mit Personendaten bei vorhandenem Eintrag und `404 Not Found` bei nicht vorhandenem.

- **GetByColor**  
  Testet die korrekte Filterung nach Farbe und Rückgabe auch bei leerer Ergebnisliste.

- **AddPerson**  
  Validiert:
  - `400 Bad Request` bei null-Input.
  - `201 Created` mit Location-Header bei erfolgreicher Erstellung.

---

## Test Setup & Ausführung

- Die Tests sind in einem separaten Testprojekt `PersonApi.Test` implementiert.
- Mocking wird für DbContext und Repository-Abhängigkeiten verwendet, um die Tests unabhängig von der tatsächlichen Datenquelle zu machen.
- CSV-Dateien werden für Tests ggf. temporär kopiert oder gemockt.
- Die Tests können mit dem Befehl `dotnet test` im Projektverzeichnis ausgeführt werden.

NuGet-Paket `Microsoft.EntityFrameworkCore.InMemory` installieren: 
```bash
    dotnet add PersonApi.Test package Microsoft.EntityFrameworkCore.InMemory 
```

Moq installieren:
```bash 
dotnet add package Moq
```
oder über Visual Studio: 
1. Rechtsklick auf dein Testprojekt (`PersonApi.Test`)
2. NuGet-Pakete verwalten... auswählen
3. Nach Moq suchen
4. Paket Moq auswählen und installieren
---

## Anforderungen

- .NET SDK (Version passend zu deinem Projekt, z.B. .NET 7)
- xUnit als Test-Framework
- Moq (oder ein anderes Mocking-Framework) für die Mock-Objekte

---