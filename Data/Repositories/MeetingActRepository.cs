using Microsoft.Data.Sqlite;
using System.Globalization;
using ACTA.Models;
using ACTA.Utilities;

namespace ACTA.Data.Repositories;

public sealed class MeetingActRepository
{
    private readonly Database _database;

    public MeetingActRepository(Database database)
    {
        _database = database;
    }

    public async Task<long> InsertAsync(MeetingAct meetingAct)
    {
        ArgumentNullException.ThrowIfNull(meetingAct);

        await using SqliteConnection connection =
            _database.CreateConnection();

        await connection.OpenAsync();

        using SqliteTransaction transaction =
            connection.BeginTransaction();

        try
        {
            long meetingActId = await InsertMeetingActAsync(
                connection,
                transaction,
                meetingAct
            );

            await InsertParticipantsAsync(
                connection,
                transaction,
                meetingActId,
                meetingAct.Participants
            );

            transaction.Commit();

            return meetingActId;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static async Task<long> InsertMeetingActAsync(
        SqliteConnection connection,
        SqliteTransaction transaction,
        MeetingAct meetingAct)
    {
        const string sql = """
            INSERT INTO meeting_acts (
                city,
                meeting_date,
                meeting_time,
                motives,
                agreements,
                commitments,
                generator_version
            )
            VALUES (
                $city,
                $meetingDate,
                $meetingTime,
                $motives,
                $agreements,
                $commitments,
                $generatorVersion
            );

            SELECT last_insert_rowid();
            """;

        await using SqliteCommand command =
            connection.CreateCommand();

        command.Transaction = transaction;
        command.CommandText = sql;
        command.Parameters.AddWithValue("$city", meetingAct.Header.City);
        command.Parameters.AddWithValue("$meetingDate", meetingAct.Header.DateTime.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$meetingTime", meetingAct.Header.DateTime.ToString("HH:mm", CultureInfo.InvariantCulture));
        command.Parameters.AddWithValue("$motives", meetingAct.Motives);
        command.Parameters.AddWithValue("$agreements", meetingAct.Agreements);
        command.Parameters.AddWithValue("$commitments", meetingAct.Commitments);
        command.Parameters.AddWithValue("$generatorVersion", meetingAct.GeneratorVersion);

        object? result = await command.ExecuteScalarAsync();

        if (result is null)
        {
            throw new InvalidOperationException("No fue posible obtener el ID del acta.");
        }

        return Convert.ToInt64(result);
    }

    private static async Task InsertParticipantsAsync(SqliteConnection connection, SqliteTransaction transaction, long meetingActId, IReadOnlyList<Participant> participants)
    {
        const string sql = """
            INSERT INTO act_participants (
                meeting_act_id,
                full_name,
                role,
                run,
                phone,
                sort_order
            )
            VALUES (
                $meetingActId,
                $fullName,
                $role,
                $run,
                $phone,
                $sortOrder
            );
            """;

        for (int index = 0; index < participants.Count; index++)
        {
            Participant participant = participants[index];

            await using SqliteCommand command = connection.CreateCommand();
            command.Transaction = transaction;
            command.CommandText = sql;
            command.Parameters.AddWithValue("$meetingActId", meetingActId);
            command.Parameters.AddWithValue("$fullName", participant.Name);
            command.Parameters.AddWithValue("$role", participant.Role);
            command.Parameters.AddWithValue("$run",RunFormatter.Normalize(participant.Run));
            command.Parameters.AddWithValue("$phone", participant.Phone);
            command.Parameters.AddWithValue("$sortOrder", index);
            await command.ExecuteNonQueryAsync();
        }
    }

    public async Task<MeetingAct?> GetByIdAsync(long id)
    {
        await using SqliteConnection connection = _database.CreateConnection();

        await connection.OpenAsync();

        MeetingAct? meetingAct = await GetMeetingActAsync(connection, id);

        if (meetingAct is null)
        {
            return null;
        }

        meetingAct.Participants = await GetParticipantsAsync(connection, id);

        return meetingAct;
    }

    private static async Task<MeetingAct?> GetMeetingActAsync(SqliteConnection connection,long id)
    {
        const string sql = """
        SELECT
            city,
            meeting_date,
            meeting_time,
            motives,
            agreements,
            commitments,
            generator_version
        FROM meeting_acts
        WHERE id = $id;
        """;

        await using SqliteCommand command = connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue("$id", id);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return null;
        }

        string dateText = reader.GetString(reader.GetOrdinal("meeting_date"));

        string timeText = reader.GetString(reader.GetOrdinal("meeting_time"));

        DateTime dateTime = DateTime.ParseExact($"{dateText} {timeText}", "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        return new MeetingAct
        {
            Header = new Header{DateTime = dateTime},
            Motives = reader.GetString(reader.GetOrdinal("motives")),
            Agreements = reader.GetString(reader.GetOrdinal("agreements")),
            Commitments = reader.GetString(reader.GetOrdinal("commitments")),
            GeneratorVersion = reader.GetInt32(reader.GetOrdinal("generator_version"))
        };
    }

    private static async Task<List<Participant>> GetParticipantsAsync(SqliteConnection connection, long meetingActId)
    {
        const string sql = """
        SELECT
            full_name,
            role,
            run,
            phone
        FROM act_participants
        WHERE meeting_act_id = $meetingActId
        ORDER BY sort_order ASC;
        """;

        await using SqliteCommand command = connection.CreateCommand();

        command.CommandText = sql;

        command.Parameters.AddWithValue("$meetingActId", meetingActId);

        await using SqliteDataReader reader =
            await command.ExecuteReaderAsync();

        List<Participant> participants = [];

        while (await reader.ReadAsync())
        {
            participants.Add(
                new Participant
                {
                    Name = reader.GetString(reader.GetOrdinal("full_name")),
                    Role = reader.GetString(reader.GetOrdinal("role")),
                    Run = RunFormatter.Format(reader.GetString(reader.GetOrdinal("run"))),
                    Phone = reader.GetString(reader.GetOrdinal("phone"))
                }
            );
        }

        return participants;
    }
}