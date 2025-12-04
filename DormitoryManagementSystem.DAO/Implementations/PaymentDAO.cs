using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.SearchCriteria; // Criteria
using DormitoryManagementSystem.DTO.Utils; // AppConstants
using DormitoryManagementSystem.Entity;
using DormitoryManagementSystem.Utils;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class PaymentDAO : IPaymentDAO
    {
        private readonly PostgreDbContext _context;
        public PaymentDAO(PostgreDbContext context) => _context = context;

        // --- CRUD ---
        public async Task<Payment?> GetPaymentByIDAsync(string id) => await _context.Payments.FindAsync(id);

        public async Task AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();
        }

        public async Task RemovePaymentAsync(string id)
        {
            var p = await _context.Payments.FindAsync(id);
            if (p != null)
            {
                _context.Payments.Remove(p);
                await _context.SaveChangesAsync();
            }
        }

        // --- SEARCH ALL-IN-ONE ---
        public async Task<IEnumerable<Payment>> SearchPaymentsAsync(PaymentSearchCriteria criteria)
        {
            var query = _context.Payments.AsNoTracking()
                .Include(p => p.Contract).ThenInclude(c => c.Student) // Join lấy tên SV
                .Include(p => p.Contract).ThenInclude(c => c.Room)    // Join lấy phòng/tòa nhà
                .AsQueryable();

            // 1. Lọc ID quan hệ
            if (!string.IsNullOrEmpty(criteria.ContractID))
                query = query.Where(p => p.Contractid == criteria.ContractID);

            if (!string.IsNullOrEmpty(criteria.StudentID))
                query = query.Where(p => p.Contract.Studentid == criteria.StudentID);

            if (!string.IsNullOrEmpty(criteria.BuildingID) && criteria.BuildingID != "All")
                query = query.Where(p => p.Contract.Room.Buildingid == criteria.BuildingID);

            // 2. Lọc thời gian
            if (criteria.Month.HasValue && criteria.Month > 0)
                query = query.Where(p => p.Billmonth == criteria.Month.Value);

            if (criteria.Year.HasValue && criteria.Year > 0)
                query = query.Where(p => p.Paymentdate.HasValue && p.Paymentdate.Value.Year == criteria.Year.Value);

            // 3. Lọc trạng thái
            if (!string.IsNullOrEmpty(criteria.Status) && criteria.Status != "All")
            {
                if (criteria.Status == "Pending") // Logic: Pending = Unpaid OR Late
                    query = query.Where(p => p.Paymentstatus == AppConstants.PaymentStatus.Unpaid || p.Paymentstatus == AppConstants.PaymentStatus.Late);
                else
                    query = query.Where(p => p.Paymentstatus == criteria.Status);
            }

            // 4. Tìm kiếm từ khóa (Keyword)
            if (!string.IsNullOrWhiteSpace(criteria.Keyword))
            {
                string key = criteria.Keyword.ToLower().Trim();
                query = query.Where(p => p.Contract.Student.Fullname.ToLower().Contains(key) ||
                                         p.Contract.Studentid.ToLower().Contains(key));
            }

            return await query.OrderByDescending(p => p.Billmonth)
                              .ThenByDescending(p => p.Paymentdate)
                              .ToListAsync();
        }
    }
}