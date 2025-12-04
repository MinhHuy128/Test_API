using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DormitoryManagementSystem.DTO.Dashboard;

namespace DormitoryManagementSystem.DAO.Interfaces
{
    public interface IDashboardDAO
    {
        // Thống kê từng tòa nhà
        Task<List<BuildingKpiDTO>> GetBuildingStatsAsync();

        // KPI tổng quan (Header dashboard)
        Task<DashboardKpiDTO> GetGeneralKpiAsync(string? buildingFilter, DateTime from, DateTime to);

        // Dữ liệu biểu đồ
        Task<DashboardChartsDTO> GetChartDataAsync(string? buildingFilter, DateTime from, DateTime to);

        // Cảnh báo (Alerts)
        Task<List<AlertDTO>> GetPaymentAlertsAsync();
        Task<List<AlertDTO>> GetViolationAlertsAsync();

        // Hoạt động gần đây (Activities)
        Task<List<ActivityDTO>> GetRecentContractsAsync(int limit);
        Task<List<ActivityDTO>> GetRecentPaymentsAsync(int limit);
        Task<List<ActivityDTO>> GetRecentViolationsAsync(int limit);
    }
}