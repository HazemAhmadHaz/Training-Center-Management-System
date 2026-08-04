using Microsoft.EntityFrameworkCore;
using TrainingCenterAPI.Data;
using TrainingCenterAPI.Models;
using TrainingCenterAPI.Repositories.Interfaces;

namespace TrainingCenterAPI.Repositories.Implementations
{
    public class PersonRepository : IPersonRepository
    {
        private readonly TrainingCenterDbContext _context;

        public PersonRepository(TrainingCenterDbContext context)
        {
            _context = context;
        }

        public async Task<Person?> GetByEmailAsync(string email)
        {
            return await _context.People
                .FirstOrDefaultAsync(p => p.Email == email);
        }

        public async Task<Person?> GetByIdAsync(int id)
        {
            return await _context.People
                .FirstOrDefaultAsync(p => p.PersonId == id);
        }

        public async Task AddAsync(Person person)
        {
            await _context.People.AddAsync(person);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}