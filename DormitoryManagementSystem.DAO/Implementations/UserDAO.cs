using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class UserDAO : IUserDAO
    {
        private readonly PostgreDbContext _context;
        public UserDAO(PostgreDbContext context) => _context = context;

        public async Task<IEnumerable<User>> GetAllUsersAsync() =>
            await _context.Users.AsNoTracking().Where(x => x.IsActive).ToListAsync();

        public async Task<IEnumerable<User>> GetAllUsersIncludingInactivesAsync() =>
            await _context.Users.AsNoTracking().ToListAsync();

        public async Task<User?> GetUserByIDAsync(string id) => await _context.Users.FindAsync(id);

        public async Task<User?> GetUserByUsernameAsync(string username) =>
            await _context.Users.AsNoTracking()
                                .Include(u => u.Student)
                                .FirstOrDefaultAsync(user => user.Username == username);

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(string id)
        {
            var u = await _context.Users.FindAsync(id);
            if (u != null)
            {
                u.IsActive = false;
                await _context.SaveChangesAsync();
            }
        }
    }
}