using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.Repositories.Interfaces;

/// <summary>
/// Generic implementation of IRepository&lt;T&gt; using EF Core's DbContext.Set&lt;T&gt;()
/// to perform basic CRUD against any entity type. Entity-specific repositories
/// (e.g. CourseRepository) inherit from this to get GetAll/GetById/Add/Update/
/// Delete/SaveChanges for free, without rewriting them.
/// </summary>

/// <summary>
/// Principle: Single Responsibility + Open/Closed.
/// Its only job is generic CRUD via EF Core. Adding a new entity means
/// creating a new repository class that inherits from this one —
/// this class itself never needs to change.
/// </summary>

///<summary>
///Both of these just mark the entity as "modified" or "to be removed" in memory — EF Core's change tracker. No actual database call happens here.
///The real database work only happens later, when you call SaveChangesAsync() — that's the one that actually sends SQL to the server,
///which is why that one is legitimately async (Task<bool>) and named accordingly.
///The naming convention in .NET (and C# in general): a method should only be named ...Async if it returns a Task/Task<T>
///(meaning it can be awaited, meaning it does real asynchronous work like I/O — database calls, file access, network calls).
///GetAllAsync, AddAsync, SaveChangesAsync all touch the database → legitimately async → correctly named.
///Update/Delete only change an in-memory tracked state → not actually async → correctly not named ...Async.
///</summary>>

namespace TrainingCenterAPI.Repositories.Implementations
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly TrainingCenterDbContext _context;

        public Repository(TrainingCenterDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id)
        {
            return await _context.Set<T>().FindAsync(id);
        }

        public async Task AddAsync(T entity)
        {
            await _context.Set<T>().AddAsync(entity);
        }

        public void Update(T entity)
        {
            _context.Set<T>().Update(entity);
        }

        public void Delete(T entity)
        {
            _context.Set<T>().Remove(entity);
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}