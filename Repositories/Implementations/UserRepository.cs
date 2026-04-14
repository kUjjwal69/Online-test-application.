using Microsoft.EntityFrameworkCore;
using TestManagementApplication.Data;
using TestManagementApplication.Models.Entities;
using TestManagementApplication.Repositories.Generic;
using TestManagementApplication.Repositories.Interfaces;

namespace TestManagementApplication.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByIdAsync(Guid id)
            => await _context.Users.FindAsync(id);

        public async Task<User?> GetByUsernameAsync(string username)
            => await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

        public async Task<User?> GetByEmailAsync(string email)
            => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

        public async Task<IEnumerable<User>> GetAllCandidatesAsync()
            => await _context.Users.Where(u => u.Role == "User").ToListAsync();

        public async Task<User> CreateAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(Guid id)
            => await _context.Users.AnyAsync(u => u.Id == id);
    }
}
