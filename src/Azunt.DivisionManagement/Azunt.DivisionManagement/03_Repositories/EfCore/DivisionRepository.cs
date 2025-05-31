using Azunt.Models.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Azunt.DivisionManagement
{
    public class DivisionRepository : IDivisionRepository
    {
        private readonly DivisionDbContextFactory _factory;
        private readonly ILogger<DivisionRepository> _logger;

        public DivisionRepository(
            DivisionDbContextFactory factory,
            ILoggerFactory loggerFactory)
        {
            _factory = factory;
            _logger = loggerFactory.CreateLogger<DivisionRepository>();
        }

        private DivisionDbContext CreateContext(string? connectionString)
        {
            return string.IsNullOrEmpty(connectionString)
                ? _factory.CreateDbContext()
                : _factory.CreateDbContext(connectionString);
        }

        public async Task<Division> AddAsync(Division model, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);
            model.CreatedAt = DateTime.UtcNow;
            context.Divisions.Add(model);
            await context.SaveChangesAsync();
            return model;
        }

        public async Task<List<Division>> GetAllAsync(string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);
            return await context.Divisions
                .OrderByDescending(m => m.Id)
                .ToListAsync();
        }

        public async Task<Division> GetByIdAsync(long id, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);
            return await context.Divisions
                       .SingleOrDefaultAsync(m => m.Id == id)
                   ?? new Division();
        }

        public async Task<bool> UpdateAsync(Division model, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);
            context.Attach(model);
            context.Entry(model).State = EntityState.Modified;
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteAsync(long id, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);
            var entity = await context.Divisions.FindAsync(id);
            if (entity == null) return false;
            context.Divisions.Remove(entity);
            return await context.SaveChangesAsync() > 0;
        }

        public async Task<ArticleSet<Division, int>> GetArticlesAsync<TParentIdentifier>(
            int pageIndex, int pageSize, string searchField, string searchQuery, string sortOrder, TParentIdentifier parentIdentifier, string? connectionString = null)
        {
            await using var context = CreateContext(connectionString);

            var query = context.Divisions.AsQueryable();

            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(m => m.Name!.Contains(searchQuery));
            }

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(m => m.Id)
                .Skip(pageIndex * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new ArticleSet<Division, int>(items, totalCount);
        }
    }
}