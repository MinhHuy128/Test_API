using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Contracts; // Criteria
using DormitoryManagementSystem.DTO.SearchCriteria;
using DormitoryManagementSystem.DTO.Utils; // AppConstants
using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.Utils;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class ContractDAO : IContractDAO
    {
        private readonly PostgreDbContext _context;
        public ContractDAO(PostgreDbContext context) => _context = context;

        // --- CRUD ---
        public async Task<Contract?> GetContractByIDAsync(string id) => await _context.Contracts.FindAsync(id);

        public async Task AddContractAsync(Contract contract)
        {
            await _context.Contracts.AddAsync(contract);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateContractAsync(Contract contract)
        {
            _context.Contracts.Update(contract);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteContractAsync(string id)
        {
            var c = await _context.Contracts.FindAsync(id);
            if (c != null)
            {
                c.Status = AppConstants.ContractStatus.Terminated; // Soft Delete
                await _context.SaveChangesAsync();
            }
        }

        // --- LOGIC CHECKS ---
        public async Task<Contract?> GetActiveContractByStudentIDAsync(string studentID) =>
            await _context.Contracts.AsNoTracking()
                                    .Where(c => c.Studentid == studentID && c.Status == AppConstants.ContractStatus.Active)
                                    .FirstOrDefaultAsync();

        public async Task<Contract?> GetContractDetailAsync(string studentId) =>
            await _context.Contracts.AsNoTracking()
                .Include(c => c.Student)
                .Include(c => c.Room).ThenInclude(r => r.Building)
                .Where(c => c.Studentid == studentId && c.Status == AppConstants.ContractStatus.Active)
                .FirstOrDefaultAsync();

        // --- SEARCH ALL-IN-ONE ---
        public async Task<IEnumerable<Contract>> SearchContractsAsync(ContractSearchCriteria criteria)
        {
            var query = _context.Contracts.AsNoTracking()
                .Include(c => c.Student)
                .Include(c => c.Room).ThenInclude(r => r.Building) // Include sâu để lấy tên tòa nhà luôn
                .AsQueryable();

            // Default: Không lấy HĐ đã hủy (trừ khi có yêu cầu khác)
            if (string.IsNullOrEmpty(criteria.Status))
                query = query.Where(c => c.Status != AppConstants.ContractStatus.Terminated);
            else if (criteria.Status != "All")
                query = query.Where(c => c.Status == criteria.Status);

            if (!string.IsNullOrEmpty(criteria.StudentID))
                query = query.Where(c => c.Studentid == criteria.StudentID);

            if (!string.IsNullOrEmpty(criteria.BuildingID) && criteria.BuildingID != "All")
                query = query.Where(c => c.Room.Buildingid == criteria.BuildingID);

            if (criteria.FromDate.HasValue)
                query = query.Where(c => c.Starttime >= criteria.FromDate.Value);

            if (criteria.ToDate.HasValue)
                query = query.Where(c => c.Endtime <= criteria.ToDate.Value);

            if (!string.IsNullOrWhiteSpace(criteria.SearchTerm))
            {
                string key = criteria.SearchTerm.ToLower().Trim();
                query = query.Where(c => c.Contractid.ToLower().Contains(key) ||
                                         c.Student.Fullname.ToLower().Contains(key) ||
                                         c.Student.Studentid.ToLower().Contains(key));
            }

            return await query.OrderByDescending(c => c.Createddate).ToListAsync();
        }
    }
}