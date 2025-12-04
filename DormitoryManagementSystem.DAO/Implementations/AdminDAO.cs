using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class AdminDAO : IAdminDAO
    {
        private readonly PostgreDbContext _context;
        public AdminDAO(PostgreDbContext context) => _context = context;

        public async Task<IEnumerable<Admin>> GetAllAdminsAsync() =>
            await _context.Admins.AsNoTracking()
                                 .Include(a => a.User)
                                 .Where(a => a.User.IsActive)
                                 .ToListAsync();

        public async Task<IEnumerable<Admin>> GetAllAdminsIncludingInactivesAsync() =>
            await _context.Admins.AsNoTracking().ToListAsync();

        public async Task<Admin?> GetAdminByIDAsync(string id) => await _context.Admins.FindAsync(id);

        public async Task<Admin?> GetAdminByUserIDAsync(string userId) =>
            await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Userid == userId);

        public async Task<Admin?> GetAdminByCCCDAsync(string cccd) =>
            await _context.Admins.AsNoTracking().FirstOrDefaultAsync(a => a.Idcard == cccd);

        public async Task AddAdminAsync(Admin admin)
        {
            await _context.Admins.AddAsync(admin);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAdminAsync(Admin admin)
        {
            _context.Admins.Update(admin);
            await _context.SaveChangesAsync();
        }
    }
}