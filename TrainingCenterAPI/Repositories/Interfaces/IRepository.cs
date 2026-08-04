using System.Linq.Expressions;

/// <summary>
/// Generic repository contract for basic CRUD operations shared by all entities.
/// Defined once and reused across Course, Student, Instructor, and Enrollment
/// to avoid repeating identical Get/Add/Update/Delete methods in every
/// entity-specific repository. Entity-specific repositories inherit from
/// this and add only what's unique to that entity.
/// </summary>

/// <summary>
/// Principle: Interface Segregation + Open/Closed.
/// Defines only the CRUD operations every entity genuinely shares.
/// New entities implement it without needing changes to this interface,
/// and no entity is forced to depend on methods it doesn't use.
/// </summary>

namespace TrainingCenterAPI.Repositories.Interfaces
{
    public interface IRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task<bool> SaveChangesAsync();
    }
}