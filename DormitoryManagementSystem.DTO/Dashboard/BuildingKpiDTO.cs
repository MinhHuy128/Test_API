namespace DormitoryManagementSystem.DTO.Dashboard
{
    public class BuildingKpiDTO
    {
        public string BuildingName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public int OccupiedRooms { get; set; }
        public int TotalRooms { get; set; }
        public decimal OccupancyRate { get; set; }
    }
}