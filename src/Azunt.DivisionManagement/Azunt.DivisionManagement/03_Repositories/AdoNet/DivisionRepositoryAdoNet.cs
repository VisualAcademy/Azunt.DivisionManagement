using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Azunt.Models.Common;

namespace Azunt.DivisionManagement;

public class DivisionRepositoryAdoNet : IDivisionRepository
{
    private readonly string _defaultConnectionString;
    private readonly ILogger<DivisionRepositoryAdoNet> _logger;

    public DivisionRepositoryAdoNet(string defaultConnectionString, ILoggerFactory loggerFactory)
    {
        _defaultConnectionString = defaultConnectionString;
        _logger = loggerFactory.CreateLogger<DivisionRepositoryAdoNet>();
    }

    private SqlConnection GetConnection(string? connectionString)
    {
        return new SqlConnection(connectionString ?? _defaultConnectionString);
    }

    public async Task<Division> AddAsync(Division model, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"INSERT INTO Divisions (Active, CreatedAt, CreatedBy, Name)
                            OUTPUT INSERTED.Id
                            VALUES (@Active, @CreatedAt, @CreatedBy, @Name)";

        cmd.Parameters.AddWithValue("@Active", model.Active ?? true);
        cmd.Parameters.AddWithValue("@CreatedAt", DateTimeOffset.UtcNow);
        cmd.Parameters.AddWithValue("@CreatedBy", model.CreatedBy ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);

        await conn.OpenAsync();
        var result = await cmd.ExecuteScalarAsync();
        if (result == null || result == DBNull.Value)
        {
            throw new InvalidOperationException("Failed to insert Division. No ID was returned.");
        }

        model.Id = Convert.ToInt64(result);
        return model;
    }

    public async Task<List<Division>> GetAllAsync(string? connectionString = null)
    {
        var result = new List<Division>();

        var conn = GetConnection(connectionString);
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Active, CreatedAt, CreatedBy, Name FROM Divisions ORDER BY Id DESC";

        await conn.OpenAsync();
        var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new Division
            {
                Id = reader.GetInt64(0),
                Active = reader.IsDBNull(1) ? (bool?)null : reader.GetBoolean(1),
                CreatedAt = reader.GetDateTimeOffset(2),
                CreatedBy = reader.IsDBNull(3) ? null : reader.GetString(3),
                Name = reader.IsDBNull(4) ? null : reader.GetString(4)
            });
        }
        return result;
    }

    public async Task<Division> GetByIdAsync(long id, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, Active, CreatedAt, CreatedBy, Name FROM Divisions WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Division
            {
                Id = reader.GetInt64(0),
                Active = reader.IsDBNull(1) ? (bool?)null : reader.GetBoolean(1),
                CreatedAt = reader.GetDateTimeOffset(2),
                CreatedBy = reader.IsDBNull(3) ? null : reader.GetString(3),
                Name = reader.IsDBNull(4) ? null : reader.GetString(4)
            };
        }

        return new Division(); // 빈 모델 반환
    }

    public async Task<bool> UpdateAsync(Division model, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var cmd = conn.CreateCommand();
        cmd.CommandText = @"UPDATE Divisions SET
                                Active = @Active,
                                Name = @Name
                            WHERE Id = @Id";

        cmd.Parameters.AddWithValue("@Active", model.Active ?? true);
        cmd.Parameters.AddWithValue("@Name", model.Name ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@Id", model.Id);

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<bool> DeleteAsync(long id, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Divisions WHERE Id = @Id";
        cmd.Parameters.AddWithValue("@Id", id);

        await conn.OpenAsync();
        return await cmd.ExecuteNonQueryAsync() > 0;
    }

    public async Task<ArticleSet<Division, int>> GetArticlesAsync<TParentIdentifier>(
        int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier, string? connectionString = null)
    {
        // 심플 버전
        var result = await GetAllAsync(connectionString);
        var filtered = string.IsNullOrWhiteSpace(searchQuery)
            ? result
            : result.Where(m => m.Name != null && m.Name.Contains(searchQuery)).ToList();

        var paged = filtered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        return new ArticleSet<Division, int>(paged, filtered.Count);
    }
}