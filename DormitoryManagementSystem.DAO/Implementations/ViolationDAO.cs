using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.DTO.SearchCriteria; // Criteria
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class ViolationDAO : IViolationDAO
    {
        private readonly PostgreDbContext _context;
        public ViolationDAO(PostgreDbContext context) => _context = context;

        // --- CRUD ---
        public async Task<Violation?> GetViolationByIdAsync(string id) => await _context.Violations.FindAsync(id);

        public async Task AddNewViolationAsync(Violation violation)
        {
            await _context.Violations.AddAsync(violation);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateViolationAsync(Violation violation)
        {
            _context.Violations.Update(violation);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteViolationAsync(string id)
        {
            var v = await _context.Violations.FindAsync(id);
            if (v != null)
            {
                _context.Violations.Remove(v);
                await _context.SaveChangesAsync();
            }
        }

        // --- SEARCH ALL-IN-ONE ---
        public async Task<IEnumerable<Violation>> SearchViolationsAsync(ViolationSearchCriteria criteria)
        {
            var query = _context.Violations.AsNoTracking()
                .Include(v => v.Student) // Join Student để lấy tên
                .Include(v => v.Room)    // Join Room để lấy số phòng
                .AsQueryable();

            if (!string.IsNullOrEmpty(criteria.StudentID))
                query = query.Where(v => v.Studentid == criteria.StudentID);

            if (!string.IsNullOrEmpty(criteria.RoomID) && criteria.RoomID != "All")
                query = query.Where(v => v.Roomid == criteria.RoomID);

            if (!string.IsNullOrEmpty(criteria.Status) && criteria.Status != "All")
                query = query.Where(v => v.Status == criteria.Status);

            // Keyword Search: Tìm theo Tên SV hoặc Mã SV
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                string key = criteria.Keyword.ToLower().Trim();
                query = query.Where(v => (v.Student != null && v.Student.Fullname.ToLower().Contains(key)) ||
                                         (v.Studentid != null && v.Studentid.ToLower().Contains(key)));
            }

            return await query.OrderByDescending(v => v.Violationdate).ToListAsync();
        }
    }
}