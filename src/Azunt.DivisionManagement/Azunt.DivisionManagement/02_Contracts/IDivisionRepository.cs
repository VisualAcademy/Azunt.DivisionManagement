using Azunt.Models.Common;

namespace Azunt.DivisionManagement;

public interface IDivisionRepository
{
    Task<Division> AddAsync(Division model, string? connectionString = null);
    Task<List<Division>> GetAllAsync(string? connectionString = null);
    Task<Division> GetByIdAsync(long id, string? connectionString = null);
    Task<bool> UpdateAsync(Division model, string? connectionString = null);
    Task<bool> DeleteAsync(long id, string? connectionString = null);
    Task<ArticleSet<Division, int>> GetArticlesAsync<TParentIdentifier>(int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier, string? connectionString = null);
}
