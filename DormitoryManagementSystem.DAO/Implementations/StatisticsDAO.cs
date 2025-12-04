using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Statistics;
using DormitoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implementations
{
    public class StatisticsDAO : IStatisticsDAO
    {
        private readonly PostgreDbContext _context;
        public StatisticsDAO(PostgreDbContext context) => _context = context;

        public async Task<DashboardStatsDTO> GetDashboardStatsAsync()
        {
            int currentYear = DateTime.Now.Year;
            var stats = new DashboardStatsDTO { TotalBuildings = await _context.Buildings.CountAsync() };

            var roomStats = await _context.Rooms.AsNoTracking()
                .Where(r => r.Status == "Active")
                .GroupBy(x => 1)
                .Select(g => new { TotalRooms = g.Count(), TotalCapacity = g.Sum(r => r.Capacity), TotalOccupancy = g.Sum(r => r.Currentoccupancy ?? 0) })
                .FirstOrDefaultAsync();

            if (roomStats != null)
            {
                stats.TotalRooms = roomStats.TotalRooms;
                stats.TotalStudents = roomStats.TotalOccupancy;
                if (roomStats.TotalCapacity > 0)
                    stats.OccupancyRate = Math.Round((double)roomStats.TotalOccupancy / roomStats.TotalCapacity * 100, 2);
            }
            stats.YearlyRevenue = await _context.Payments.AsNoTracking()
                .Where(p => p.Paymentstatus == "Paid" && p.Paymentdate.HasValue && p.Paymentdate.Value.Year == currentYear)
                .SumAsync(p => p.Paymentamount);
            return stats;
        }

        public async Task<IEnumerable<RevenueStatsDTO>> GetMonthlyRevenueAsync(int year) =>
            await _context.Payments.AsNoTracking()
                .Where(p => p.Paymentdate.HasValue && p.Paymentdate.Value.Year == year)
                .GroupBy(p => p.Paymentdate.Value.Month)
                .Select(g => new RevenueStatsDTO { Month = g.Key, Year = year, Revenue = g.Sum(p => p.Paidamount) })
                .OrderBy(s => s.Month).ToListAsync();

        public async Task<IEnumerable<Contract>> GetContractsByYearAsync(int year)
        {
            var firstDay = DateOnly.FromDateTime(new DateTime(year, 1, 1));
            var lastDay = DateOnly.FromDateTime(new DateTime(year, 12, 31));
            return await _context.Contracts.AsNoTracking()
                .Where(c => c.Status != "Terminated" && c.Starttime <= lastDay && c.Endtime >= firstDay)
                .ToListAsync();
        }

        public async Task<GenderStatsDTO> GetGenderStatsAsync()
        {
            var stats = await _context.Students.AsNoTracking()
                .GroupBy(s => s.Gender).Select(g => new { Gender = g.Key, Count = g.Count() }).ToListAsync();
            var result = new GenderStatsDTO();
            foreach (var item in stats)
            {
                if (item.Gender == "Male") result.MaleCount = item.Count;
                else if (item.Gender == "Female") result.FemaleCount = item.Count;
            }
            return result;
        }

        public async Task<IEnumerable<BuildingComparisonDTO>> GetBuildingComparisonAsync(int? year) =>
            await _context.Buildings.AsNoTracking()
                .Select(b => new BuildingComparisonDTO
                {
                    BuildingID = b.Buildingid,
                    BuildingName = b.Buildingname,
                    TotalStudents = b.Rooms.SelectMany(r => r.Contracts).Count(c => c.Status == "Active"),
                    TotalRevenue = b.Rooms.SelectMany(r => r.Contracts).SelectMany(c => c.Payments)
                        .Where(p => p.Paymentstatus == "Paid" && (!year.HasValue || (p.Paymentdate.HasValue && p.Paymentdate.Value.Year == year)))
                        .Sum(p => p.Paymentamount)
                }).OrderByDescending(x => x.TotalRevenue).ToListAsync();

        public async Task<IEnumerable<ViolationTrendDTO>> GetViolationTrendAsync(int year) =>
            await _context.Violations.AsNoTracking()
                .Where(v => v.Violationdate.HasValue && v.Violationdate.Value.Year == year)
                .GroupBy(v => v.Violationdate.Value.Month)
                .Select(g => new ViolationTrendDTO { Month = g.Key, Year = year, ViolationCount = g.Count() })
                .OrderBy(s => s.Month).ToListAsync();

        public async Task<ViolationSummaryDTO> GetViolationSummaryStatsAsync()
        {
            var pending = await _context.Violations.CountAsync(v => v.Status == "Pending");
            var resolved = await _context.Violations.CountAsync(v => v.Status == "Resolved" || v.Status == "Paid");
            return new ViolationSummaryDTO { UnprocessedCount = pending, ProcessedCount = resolved };
        }

        public async Task<PaymentStatsDTO> GetPaymentStatisticsAsync()
        {
            var stats = await _context.Payments.AsNoTracking()
                .GroupBy(p => p.Paymentstatus).Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(p => p.Paymentamount) }).ToListAsync();
            var result = new PaymentStatsDTO();
            foreach (var item in stats)
            {
                if (item.Status == "Paid") { result.PaidCount = item.Count; result.PaidTotalAmount = item.Total; }
                else if (item.Status == "Unpaid") { result.UnpaidCount = item.Count; result.UnpaidTotalAmount = item.Total; }
                else if (item.Status == "Late") { result.LateCount = item.Count; result.LateTotalAmount = item.Total; }
            }
            return result;
        }
    }
}