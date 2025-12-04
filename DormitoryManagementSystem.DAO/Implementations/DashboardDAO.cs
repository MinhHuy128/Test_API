using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DormitoryManagementSystem.DAO.Context;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Dashboard;
using DormitoryManagementSystem.Entity; // Namespace chứa các Entity bạn cung cấp
using Microsoft.EntityFrameworkCore;

namespace DormitoryManagementSystem.DAO.Implements
{
    public class DashboardDAO : IDashboardDAO
    {
        private readonly PostgreDbContext _context; // Đổi tên này theo Context của bạn

        public DashboardDAO(PostgreDbContext context)
        {
            _context = context;
        }

        // 1. Lấy thống kê chi tiết từng tòa nhà
        public async Task<List<BuildingKpiDTO>> GetBuildingStatsAsync()
        {
            return await _context.Buildings
                .AsNoTracking()
                .Select(b => new BuildingKpiDTO
                {
                    BuildingName = b.Buildingname,
                    Gender = b.Gendertype ?? "Mixed", // Xử lý null
                    // Giả sử: TotalRooms ở đây là tổng số Giường (Capacity) để tính occupancy rate
                    TotalRooms = b.Rooms.Sum(r => r.Capacity),
                    // OccupiedRooms là tổng số người đang ở
                    OccupiedRooms = b.Rooms.Sum(r => r.Currentoccupancy ?? 0),
                })
                .ToListAsync();
        }

        // 2. Lấy KPI tổng quan (4-6 ô số trên cùng)
        public async Task<DashboardKpiDTO> GetGeneralKpiAsync(string? buildingFilter, DateTime from, DateTime to)
        {
            // Query cơ sở cho Room
            var roomQuery = _context.Rooms.AsQueryable();

            // Nếu có lọc theo tòa nhà
            if (!string.IsNullOrEmpty(buildingFilter) && !buildingFilter.ToLower().StartsWith("tất cả"))
            {
                roomQuery = roomQuery.Where(r => r.Building.Buildingname.Contains(buildingFilter));
            }

            var totalRooms = await roomQuery.CountAsync();
            // Phòng được coi là Full nếu số người ở >= Sức chứa
            var occupiedRooms = await roomQuery.CountAsync(r => (r.Currentoccupancy ?? 0) >= r.Capacity);

            // Tính doanh thu trong khoảng thời gian
            var payments = await _context.Payments
                .AsNoTracking()
                .Where(p => p.Paymentdate >= from && p.Paymentdate <= to)
                .SumAsync(p => p.Paymentamount);

            // Đếm hợp đồng đang chờ duyệt
            var pendingContracts = await _context.Contracts
                .AsNoTracking()
                .CountAsync(c => c.Status == "Pending" || c.Status == "AwaitingApproval");

            // Đếm vi phạm chưa xử lý
            var openViolations = await _context.Violations
                .AsNoTracking()
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

        // 3. Lấy dữ liệu vẽ biểu đồ
        public async Task<DashboardChartsDTO> GetChartDataAsync(string? buildingFilter, DateTime from, DateTime to)
        {
            var chartData = new DashboardChartsDTO();

            // --- Biểu đồ Tròn (Pie Chart) ---
            var roomQuery = _context.Rooms.AsNoTracking();
            if (!string.IsNullOrEmpty(buildingFilter) && !buildingFilter.ToLower().StartsWith("tất cả"))
            {
                roomQuery = roomQuery.Where(r => r.Building.Buildingname.Contains(buildingFilter));
            }

            chartData.OccupiedCount = await roomQuery.CountAsync(r => (r.Currentoccupancy ?? 0) >= r.Capacity);
            chartData.AvailableCount = await roomQuery.CountAsync(r => (r.Currentoccupancy ?? 0) < r.Capacity);

            // --- Biểu đồ Cột (Bar Chart) - Thống kê theo Tòa nhà ---
            // Chỉ lấy Top 10 tòa nhà nếu quá nhiều
            chartData.OccupancyByBuilding = await _context.Buildings
                .AsNoTracking()
                .Select(b => new OccupancyByBuildingDTO
                {
                    Building = b.Buildingname,
                    Occupied = b.Rooms.Sum(r => r.Currentoccupancy ?? 0),
                    Capacity = b.Rooms.Sum(r => r.Capacity)
                })
                .OrderBy(x => x.Building)
                .ToListAsync();

            // --- Biểu đồ Đường (Line Chart) - Xu hướng Hợp đồng ---
            // Lưu ý: GroupBy Date/Week trong EF Core đôi khi lỗi translation tùy DB Provider.
            // Giải pháp an toàn: Lấy list ngày về RAM rồi group (vì số lượng record trong 1 tháng không quá lớn).
            var contractDates = await _context.Contracts
                .AsNoTracking()
                .Where(c => c.Createddate >= from && c.Createddate <= to)
                .Select(c => c.Createddate)
                .ToListAsync();

            chartData.ContractsByWeek = contractDates
                .Where(d => d.HasValue) // Lọc null
                .GroupBy(d => ISOWeek.GetWeekOfYear(d.Value))
                .Select(g => new ContractByWeekDTO
                {
                    Week = $"Tuần {g.Key}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Week)
                .ToList();

            return chartData;
        }

        // 4. Cảnh báo Thanh toán (Quá hạn hoặc trạng thái Late)
        public async Task<List<AlertDTO>> GetPaymentAlertsAsync()
        {
            var thirtyDaysAgo = DateTime.Now.AddDays(-30);

            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Contract).ThenInclude(c => c.Student) // Join bảng để lấy tên SV
                .Where(p => p.Paymentstatus == "Late" ||
                           (p.Paymentstatus != "Paid" && p.Paymentdate < thirtyDaysAgo))
                .OrderByDescending(p => p.Paymentdate)
                .Take(5) // Lấy top 5
                .Select(p => new AlertDTO
                {
                    Type = "Thanh toán",
                    Message = $"Hóa đơn {p.Paymentid} của {p.Contract.Student.Fullname} chưa thanh toán.",
                    Date = p.Paymentdate ?? DateTime.Now
                })
                .ToListAsync();
        }

        // 5. Cảnh báo Vi phạm (Mới nhất chưa xử lý)
        public async Task<List<AlertDTO>> GetViolationAlertsAsync()
        {
            return await _context.Violations
                .AsNoTracking()
                .Include(v => v.Student) // Join bảng Student
                .Where(v => v.Status != "Closed" && v.Status != "Resolved")
                .OrderByDescending(v => v.Violationdate)
                .Take(5)
                .Select(v => new AlertDTO
                {
                    Type = "Vi phạm",
                    Message = $"{(v.Student != null ? v.Student.Fullname : "Unknown")}: {v.Violationtype}",
                    Date = v.Violationdate ?? DateTime.Now
                })
                .ToListAsync();
        }

        // 6. Helper: Lấy hoạt động Hợp đồng gần đây
        public async Task<List<ActivityDTO>> GetRecentContractsAsync(int limit)
        {
            return await _context.Contracts
                .AsNoTracking()
                .Include(c => c.Student)
                .OrderByDescending(c => c.Createddate)
                .Take(limit)
                .Select(c => new ActivityDTO
                {
                    Time = c.Createddate ?? DateTime.Now,
                    Description = $"Hợp đồng mới: {c.Student.Fullname} - Phòng {c.Roomid}"
                })
                .ToListAsync();
        }

        // 7. Helper: Lấy hoạt động Thanh toán gần đây
        public async Task<List<ActivityDTO>> GetRecentPaymentsAsync(int limit)
        {
            return await _context.Payments
                .AsNoTracking()
                .Include(p => p.Contract)
                .OrderByDescending(p => p.Paymentdate)
                .Take(limit)
                .Select(p => new ActivityDTO
                {
                    Time = p.Paymentdate ?? DateTime.Now,
                    Description = $"Thanh toán: {p.Paymentamount:N0} VNĐ ({p.Paymentstatus})"
                })
                .ToListAsync();
        }

        // 8. Helper: Lấy hoạt động Vi phạm gần đây
        public async Task<List<ActivityDTO>> GetRecentViolationsAsync(int limit)
        {
            return await _context.Violations
                .AsNoTracking()
                .Include(v => v.Student)
                .OrderByDescending(v => v.Violationdate)
                .Take(limit)
                .Select(v => new ActivityDTO
                {
                    Time = v.Violationdate ?? DateTime.Now,
                    Description = $"Vi phạm: {v.Violationtype} ({v.Status})"
                })
                .ToListAsync();
        }
    }
}