using Azunt.Models.Common;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace Azunt.DivisionManagement;

public class DivisionRepositoryDapper : IDivisionRepository
{
    private readonly string _defaultConnectionString;
    private readonly ILogger<DivisionRepositoryDapper> _logger;

    public DivisionRepositoryDapper(string defaultConnectionString, ILoggerFactory loggerFactory)
    {
        _defaultConnectionString = defaultConnectionString;
        _logger = loggerFactory.CreateLogger<DivisionRepositoryDapper>();
    }

    private SqlConnection GetConnection(string? connectionString)
    {
        return new SqlConnection(connectionString ?? _defaultConnectionString);
    }

    public async Task<Division> AddAsync(Division model, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var sql = @"INSERT INTO Divisions (Active, CreatedAt, CreatedBy, Name)
                    OUTPUT INSERTED.Id
                    VALUES (@Active, @CreatedAt, @CreatedBy, @Name)";

        model.CreatedAt = DateTimeOffset.UtcNow;
        model.Id = await conn.ExecuteScalarAsync<long>(sql, model);
        return model;
    }

    public async Task<List<Division>> GetAllAsync(string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var sql = "SELECT Id, Active, CreatedAt, CreatedBy, Name FROM Divisions ORDER BY Id DESC";
        var list = await conn.QueryAsync<Division>(sql);
        return list.ToList();
    }

    public async Task<Division> GetByIdAsync(long id, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var sql = "SELECT Id, Active, CreatedAt, CreatedBy, Name FROM Divisions WHERE Id = @Id";
        var model = await conn.QuerySingleOrDefaultAsync<Division>(sql, new { Id = id });
        return model ?? new Division();
    }

    public async Task<bool> UpdateAsync(Division model, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var sql = @"UPDATE Divisions SET
                        Active = @Active,
                        Name = @Name
                    WHERE Id = @Id";

        var rows = await conn.ExecuteAsync(sql, model);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(long id, string? connectionString = null)
    {
        var conn = GetConnection(connectionString);
        var sql = "DELETE FROM Divisions WHERE Id = @Id";
        var rows = await conn.ExecuteAsync(sql, new { Id = id });
        return rows > 0;
    }

    public async Task<ArticleSet<Division, int>> GetArticlesAsync<TParentIdentifier>(
        int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier, string? connectionString = null)
    {
        var all = await GetAllAsync(connectionString);
        var filtered = string.IsNullOrWhiteSpace(searchQuery)
            ? all
            : all.Where(m => m.Name != null && m.Name.Contains(searchQuery)).ToList();

        var paged = filtered
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .ToList();

        return new ArticleSet<Division, int>(paged, filtered.Count);
    }
}