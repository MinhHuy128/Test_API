using System.Collections.Generic;

namespace DormitoryManagementSystem.DTO.Dashboard
{
    public class DashboardChartsDTO
    {
        public int OccupiedCount { get; set; }
        public int AvailableCount { get; set; }
        public List<OccupancyByBuildingDTO> OccupancyByBuilding { get; set; } = new();
        public List<ContractByWeekDTO> ContractsByWeek { get; set; } = new();
    }
}