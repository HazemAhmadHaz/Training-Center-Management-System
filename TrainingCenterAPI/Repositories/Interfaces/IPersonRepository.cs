using TrainingCenterAPI.Models;

namespace TrainingCenterAPI.Repositories.Interfaces
{
    public interface IPersonRepository
    {
        Task<Person?> GetByEmailAsync(string email);
        Task<Person?> GetByIdAsync(int id);
        Task AddAsync(Person person);
        Task SaveChangesAsync();
    }
}