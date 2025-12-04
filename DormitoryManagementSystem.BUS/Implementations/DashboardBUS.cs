using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Dashboard;

namespace DormitoryManagementSystem.BUS.Implements
{
    public class DashboardBUS : IDashboardBUS
    {
        private readonly IDashboardDAO _dao;
        public DashboardBUS(IDashboardDAO dao) => _dao = dao;

        public async Task<BuildingKpiResponseDTO> GetBuildingKpisAsync()
        {
            var buildings = await _dao.GetBuildingStatsAsync();
            foreach (var b in buildings)
                b.OccupancyRate = b.TotalRooms == 0 ? 0 : Math.Round((decimal)b.OccupiedRooms * 100 / b.TotalRooms, 2);
            return new BuildingKpiResponseDTO { Buildings = buildings.OrderByDescending(k => k.OccupancyRate).ToList() };
        }

        public async Task<DashboardKpiDTO> GetDashboardKpisAsync(string? b, DateTime? from, DateTime? to) =>
            await _dao.GetGeneralKpiAsync(b, from ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), to ?? DateTime.Now);

        public async Task<DashboardChartsDTO> GetDashboardChartsAsync(string? b, DateTime? from, DateTime? to) =>
            await _dao.GetChartDataAsync(b, from ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1), to ?? DateTime.Now);

        public async Task<List<AlertDTO>> GetAlertsAsync()
        {
            // Chạy tuần tự để tránh lỗi DbContext (Concurrency Issue)
            var paymentAlerts = await _dao.GetPaymentAlertsAsync();
            var violationAlerts = await _dao.GetViolationAlertsAsync();

            var all = new List<AlertDTO>();
            all.AddRange(paymentAlerts);
            all.AddRange(violationAlerts);
            return all.OrderByDescending(a => a.Date).ToList();
        }

        public async Task<List<ActivityDTO>> GetActivitiesAsync(int limit)
        {
            int safeLimit = Math.Clamp(limit, 5, 50);
            // Chạy tuần tự
            var c = await _dao.GetRecentContractsAsync(safeLimit);
            var p = await _dao.GetRecentPaymentsAsync(safeLimit);
            var v = await _dao.GetRecentViolationsAsync(safeLimit);

            var all = new List<ActivityDTO>();
            all.AddRange(c); all.AddRange(p); all.AddRange(v);
            return all.OrderByDescending(a => a.Time).Take(safeLimit).ToList();
        }
    }
}