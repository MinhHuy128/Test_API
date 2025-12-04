using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DormitoryManagementSystem.DTO.Dashboard;

namespace DormitoryManagementSystem.BUS.Interfaces
{
    public interface IDashboardBUS
    {
        Task<BuildingKpiResponseDTO> GetBuildingKpisAsync();
        Task<DashboardKpiDTO> GetDashboardKpisAsync(string? building, DateTime? from, DateTime? to);
        Task<DashboardChartsDTO> GetDashboardChartsAsync(string? building, DateTime? from, DateTime? to);
        Task<List<AlertDTO>> GetAlertsAsync();
        Task<List<ActivityDTO>> GetActivitiesAsync(int limit);
    }
}