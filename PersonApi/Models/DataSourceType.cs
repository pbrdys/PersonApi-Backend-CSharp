namespace PersonApi.Models
{
    /// <summary>
    /// Enumeration zur Angabe der Datenquelle.
    /// </summary>
    public enum DataSourceType
    {
        /// <summary>
        /// Datenquelle ist eine CSV-Datei.
        /// </summary>
        Csv,

        /// <summary>
        /// Datenquelle ist eine Datenbank.
        /// </summary>
        Database,

        // Weitere mögliche Datenquellen könnten hier hinzugefügt werden:
        //Json,
        //RemoteApi
    }
}
