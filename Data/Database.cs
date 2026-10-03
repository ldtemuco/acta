using Microsoft.Data.Sqlite;
using System.IO;

namespace ACTA.Data;

public sealed class Database
{
    private const string DatabaseFileName = "acta.db";

    public string DatabasePath { get; }

    public string ConnectionString { get; }

    public Database()
    {
        string appDataDirectory = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            "ACTA"
        );

        Directory.CreateDirectory(appDataDirectory);

        DatabasePath = Path.Combine(
            appDataDirectory,
            DatabaseFileName
        );

        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = DatabasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            ForeignKeys = true
        };

        ConnectionString = connectionStringBuilder.ToString();
    }

    public SqliteConnection CreateConnection()
    {
        return new SqliteConnection(ConnectionString);
    }
}