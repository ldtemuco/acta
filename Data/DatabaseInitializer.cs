using Microsoft.Data.Sqlite;

namespace ACTA.Data;

public sealed class DatabaseInitializer
{
    private readonly Database _database;

    public DatabaseInitializer(Database database)
    {
        _database = database;
    }

    public async Task InitializeAsync()
    {
        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync();

        await CreateMeetingActsTableAsync(connection);
        await CreateActParticipantsTableAsync(connection);
        await CreateIndexesAsync(connection);
        await SetDatabaseVersionAsync(connection);
    }

    private static async Task CreateMeetingActsTableAsync(
        SqliteConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS meeting_acts (
                id INTEGER PRIMARY KEY AUTOINCREMENT,

                city TEXT NOT NULL DEFAULT 'TEMUCO',
                meeting_date TEXT NOT NULL,
                meeting_time TEXT NOT NULL,

                motives TEXT NOT NULL DEFAULT '',
                agreements TEXT NOT NULL DEFAULT '',
                commitments TEXT NOT NULL DEFAULT '',

                generator_version INTEGER NOT NULL DEFAULT 1,

                created_at TEXT NOT NULL DEFAULT (
                    strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
                ),

                updated_at TEXT NOT NULL DEFAULT (
                    strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
                )
            );
            """;

        await ExecuteNonQueryAsync(connection, sql);
    }

    private static async Task CreateActParticipantsTableAsync(
        SqliteConnection connection)
    {
        const string sql = """
            CREATE TABLE IF NOT EXISTS act_participants (
                id INTEGER PRIMARY KEY AUTOINCREMENT,

                meeting_act_id INTEGER NOT NULL,

                full_name TEXT NOT NULL,
                role TEXT NOT NULL DEFAULT '',
                run TEXT NOT NULL DEFAULT '',
                phone TEXT NOT NULL DEFAULT '',

                sort_order INTEGER NOT NULL DEFAULT 0,

                FOREIGN KEY (meeting_act_id)
                    REFERENCES meeting_acts(id)
                    ON DELETE CASCADE
            );
            """;

        await ExecuteNonQueryAsync(connection, sql);
    }

    private static async Task CreateIndexesAsync(
        SqliteConnection connection)
    {
        const string sql = """
            CREATE INDEX IF NOT EXISTS idx_meeting_acts_date
                ON meeting_acts(meeting_date);

            CREATE INDEX IF NOT EXISTS idx_act_participants_act
                ON act_participants(meeting_act_id);
            """;

        await ExecuteNonQueryAsync(connection, sql);
    }

    private static async Task SetDatabaseVersionAsync(
        SqliteConnection connection)
    {
        const string sql = """
            PRAGMA user_version = 1;
            """;

        await ExecuteNonQueryAsync(connection, sql);
    }

    private static async Task ExecuteNonQueryAsync(
        SqliteConnection connection,
        string sql)
    {
        await using SqliteCommand command =
            connection.CreateCommand();

        command.CommandText = sql;

        await command.ExecuteNonQueryAsync();
    }
}