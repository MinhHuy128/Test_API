using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DAO.Interfaces;
using DormitoryManagementSystem.DTO.Dashboard;

namespace DormitoryManagementSystem.BUS.Implements
{
    public class DashboardBUS : IDashboardBUS
    {
        private readonly IDashboardDAO _dashboardDAO;

        public DashboardBUS(IDashboardDAO dashboardDAO)
        {
            _dashboardDAO = dashboardDAO;
        }

        public async Task<BuildingKpiResponseDTO> GetBuildingKpisAsync()
        {
            var buildings = await _dashboardDAO.GetBuildingStatsAsync();

            foreach (var b in buildings)
            {
                b.OccupancyRate = b.TotalRooms == 0
                    ? 0
                    : Math.Round((decimal)b.OccupiedRooms * 100 / b.TotalRooms, 2);
            }

            return new BuildingKpiResponseDTO
            {
                Buildings = buildings.OrderByDescending(k => k.OccupancyRate).ToList()
            };
        }

        public async Task<DashboardKpiDTO> GetDashboardKpisAsync(string? building, DateTime? from, DateTime? to)
        {
            var dateFrom = from ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var dateTo = to ?? DateTime.Now;

            return await _dashboardDAO.GetGeneralKpiAsync(building, dateFrom, dateTo);
        }

        public async Task<DashboardChartsDTO> GetDashboardChartsAsync(string? building, DateTime? from, DateTime? to)
        {
            var dateFrom = from ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var dateTo = to ?? DateTime.Now;

            return await _dashboardDAO.GetChartDataAsync(building, dateFrom, dateTo);
        }

        // --- SỬA LỖI Ở ĐÂY: Chạy tuần tự (await từng cái một) ---
        public async Task<List<AlertDTO>> GetAlertsAsync()
        {
            // 1. Chạy query lấy Payment trước, chờ xong mới đi tiếp
            var paymentAlerts = await _dashboardDAO.GetPaymentAlertsAsync();

            // 2. Sau đó mới chạy query lấy Violation
            var violationAlerts = await _dashboardDAO.GetViolationAlertsAsync();

            var allAlerts = new List<AlertDTO>();
            allAlerts.AddRange(paymentAlerts);
            allAlerts.AddRange(violationAlerts);

            return allAlerts.OrderByDescending(a => a.Date).ToList();
        }

        // --- SỬA LỖI CẢ Ở ĐÂY LUÔN CHO CHẮC ---
        public async Task<List<ActivityDTO>> GetActivitiesAsync(int limit)
        {
            int safeLimit = Math.Clamp(limit, 5, 50);

            // Chạy tuần tự để tránh lỗi "Second operation started..."
            var contracts = await _dashboardDAO.GetRecentContractsAsync(safeLimit);
            var payments = await _dashboardDAO.GetRecentPaymentsAsync(safeLimit);
            var violations = await _dashboardDAO.GetRecentViolationsAsync(safeLimit);

            var allActivities = new List<ActivityDTO>();
            allActivities.AddRange(contracts);
            allActivities.AddRange(payments);
            allActivities.AddRange(violations);

            return allActivities
                .OrderByDescending(a => a.Time)
                .Take(safeLimit)
                .ToList();
        }
    }
}