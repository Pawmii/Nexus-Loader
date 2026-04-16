using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace NexusLoader;

public class Database
{
    private readonly string _connectionString;

    public Database(string databasePath)
    {
        var builder = new SqliteConnectionStringBuilder
        {
            DataSource = databasePath,
            Mode = SqliteOpenMode.ReadWriteCreate,
            Cache = SqliteCacheMode.Shared
        };

        _connectionString = builder.ToString();
    }

    public async Task<int> ExecuteAsync(string sql, Dictionary<string, object>? parameters = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        
        command.CommandText = sql;
        AddParameters(command, parameters);

        return await command.ExecuteNonQueryAsync().ConfigureAwait(false);
    }

    public async Task<T?> ExecuteScalarAsync<T>(string sql, Dictionary<string, object>? parameters = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        
        command.CommandText = sql;
        AddParameters(command, parameters);

        var result = await command.ExecuteScalarAsync().ConfigureAwait(false);

        if (result == null || result == DBNull.Value)
            return default;

        return (T)Convert.ChangeType(result, typeof(T));
    }

    public async Task<List<T>> QueryAsync<T>(
        string sql,
        Func<SqliteDataReader, T> mapper,
        Dictionary<string, object>? parameters = null)
    {
        var result = new List<T>();

        using var connection = new SqliteConnection(_connectionString);
        
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        
        command.CommandText = sql;
        AddParameters(command, parameters);

        using var reader = await command.ExecuteReaderAsync().ConfigureAwait(false);
        
        while (await reader.ReadAsync().ConfigureAwait(false))
        {
            result.Add(mapper(reader));
        }

        return result;
    }

    public async Task<T?> QuerySingleAsync<T>(
        string sql,
        Func<SqliteDataReader, T> mapper,
        Dictionary<string, object>? parameters = null)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        await connection.OpenAsync().ConfigureAwait(false);

        using var command = connection.CreateCommand();
        
        command.CommandText = sql;
        AddParameters(command, parameters);

        using var reader = await command.ExecuteReaderAsync(CommandBehavior.SingleRow).ConfigureAwait(false);
        
        if (await reader.ReadAsync().ConfigureAwait(false))
            return mapper(reader);

        return default;
    }

    public async Task RunInTransactionAsync(Func<SqliteConnection, SqliteTransaction, Task> action)
    {
        using var connection = new SqliteConnection(_connectionString);
        
        await connection.OpenAsync().ConfigureAwait(false);

        using var transaction = connection.BeginTransaction();
        
        try
        {
            await action(connection, transaction).ConfigureAwait(false);
            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }

    private static void AddParameters(SqliteCommand command, Dictionary<string, object>? parameters)
    {
        if (parameters == null)
            return;

        foreach (var kvp in parameters)
        {
            var parameter = command.CreateParameter();
            parameter.ParameterName = kvp.Key;
            parameter.Value = kvp.Value ?? DBNull.Value;
            command.Parameters.Add(parameter);
        }
    }
}