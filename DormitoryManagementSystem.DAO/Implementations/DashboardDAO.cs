using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Dashboard;
using DormitoryManagementSystem.Entity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace DormitoryManagementSystem.DAO.Implements
{
    public class DashboardDAO : IDashboardDAO
    {
        private readonly PostgreDbContext _context;
        public DashboardDAO(PostgreDbContext context) => _context = context;

        public async Task<List<BuildingKpiDTO>> GetBuildingStatsAsync() =>
            await _context.Buildings.AsNoTracking()
                .Select(b => new BuildingKpiDTO
                {
                    BuildingName = b.Buildingname,
                    Gender = b.Gendertype ?? "Mixed",
                    TotalRooms = b.Rooms.Sum(r => r.Capacity),
                    OccupiedRooms = b.Rooms.Sum(r => r.Currentoccupancy ?? 0)
                }).ToListAsync();

        public async Task<DashboardKpiDTO> GetGeneralKpiAsync(string? buildingFilter, DateTime from, DateTime to)
        {
            var roomQuery = _context.Rooms.AsQueryable();
            if (!string.IsNullOrEmpty(buildingFilter) && !buildingFilter.ToLower().StartsWith("tất cả"))
                roomQuery = roomQuery.Where(r => r.Building.Buildingname.Contains(buildingFilter));

            var totalRooms = await roomQuery.CountAsync();
            var occupiedRooms = await roomQuery.CountAsync(r => (r.Currentoccupancy ?? 0) >= r.Capacity);
            var payments = await _context.Payments.AsNoTracking()
                .Where(p => p.Paymentdate >= from && p.Paymentdate <= to)
                .SumAsync(p => p.Paymentamount);

            var pendingContracts = await _context.Contracts.AsNoTracking()
                .CountAsync(c => c.Status == "Pending" || c.Status == "AwaitingApproval");
            var openViolations = await _context.Violations.AsNoTracking()
                .CountAsync(v => v.Status != "Closed" && v.Status != "Resolved");

            return new DashboardKpiDTO
            {
                RoomsTotal = totalRooms,
                RoomsOccupied = occupiedRooms,
                RoomsAvailable = Math.Max(0, totalRooms - occupiedRooms),
                PaymentsThisMonth = payments,
                ContractsPending = pendingContracts,
                ViolationsOpen = openViolations
            };
        }

        public async Task<DashboardChartsDTO> GetChartDataAsync(string? buildingFilter, DateTime from, DateTime to)
        {
            var chartData = new DashboardChartsDTO();
            var roomQuery = _context.Rooms.AsNoTracking();
            if (!string.IsNullOrEmpty(buildingFilter) && !buildingFilter.ToLower().StartsWith("tất cả"))
                roomQuery = roomQuery.Where(r => r.Building.Buildingname.Contains(buildingFilter));

            chartData.OccupiedCount = await roomQuery.CountAsync(r => (r.Currentoccupancy ?? 0) >= r.Capacity);
            chartData.AvailableCount = await roomQuery.CountAsync(r => (r.Currentoccupancy ?? 0) < r.Capacity);

            chartData.OccupancyByBuilding = await _context.Buildings.AsNoTracking()
                .Select(b => new OccupancyByBuildingDTO
                {
                    Building = b.Buildingname,
                    Occupied = b.Rooms.Sum(r => r.Currentoccupancy ?? 0),
                    Capacity = b.Rooms.Sum(r => r.Capacity)
                }).OrderBy(x => x.Building).ToListAsync();

            var contractDates = await _context.Contracts.AsNoTracking()
                .Where(c => c.Createddate >= from && c.Createddate <= to)
                .Select(c => c.Createddate).ToListAsync();

            chartData.ContractsByWeek = contractDates.Where(d => d.HasValue)
                .GroupBy(d => ISOWeek.GetWeekOfYear(d.Value))
                .Select(g => new ContractByWeekDTO { Week = $"Tuần {g.Key}", Count = g.Count() })
                .OrderBy(x => x.Week).ToList();

            return chartData;
        }

        public async Task<List<AlertDTO>> GetPaymentAlertsAsync()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);
            return await _context.Payments.AsNoTracking()
                .Include(p => p.Contract).ThenInclude(c => c.Student)
                .Where(p => p.Paymentstatus == "Late" || (p.Paymentstatus != "Paid" && p.Paymentdate < thirtyDaysAgo))
                .OrderByDescending(p => p.Paymentdate).Take(5)
                .Select(p => new AlertDTO { Type = "Thanh toán", Message = $"Hóa đơn {p.Paymentid} của {p.Contract.Student.Fullname} chưa thanh toán.", Date = p.Paymentdate ?? DateTime.Now })
                .ToListAsync();
        }

        public async Task<List<AlertDTO>> GetViolationAlertsAsync() =>
            await _context.Violations.AsNoTracking().Include(v => v.Student)
                .Where(v => v.Status != "Closed" && v.Status != "Resolved")
                .OrderByDescending(v => v.Violationdate).Take(5)
                .Select(v => new AlertDTO { Type = "Vi phạm", Message = $"{(v.Student != null ? v.Student.Fullname : "Unknown")}: {v.Violationtype}", Date = v.Violationdate ?? DateTime.Now })
                .ToListAsync();

        public async Task<List<ActivityDTO>> GetRecentContractsAsync(int limit) =>
            await _context.Contracts.AsNoTracking().Include(c => c.Student)
                .OrderByDescending(c => c.Createddate).Take(limit)
                .Select(c => new ActivityDTO { Time = c.Createddate ?? DateTime.Now, Description = $"Hợp đồng mới: {c.Student.Fullname} - Phòng {c.Roomid}" })
                .ToListAsync();

        public async Task<List<ActivityDTO>> GetRecentPaymentsAsync(int limit) =>
            await _context.Payments.AsNoTracking().Include(p => p.Contract)
                .OrderByDescending(p => p.Paymentdate).Take(limit)
                .Select(p => new ActivityDTO { Time = p.Paymentdate ?? DateTime.Now, Description = $"Thanh toán: {p.Paymentamount:N0} VNĐ ({p.Paymentstatus})" })
                .ToListAsync();

        public async Task<List<ActivityDTO>> GetRecentViolationsAsync(int limit) =>
            await _context.Violations.AsNoTracking().Include(v => v.Student)
                .OrderByDescending(v => v.Violationdate).Take(limit)
                .Select(v => new ActivityDTO { Time = v.Violationdate ?? DateTime.Now, Description = $"Vi phạm: {v.Violationtype} ({v.Status})" })
                .ToListAsync();
    }
}