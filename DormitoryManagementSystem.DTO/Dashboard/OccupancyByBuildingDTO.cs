namespace DormitoryManagementSystem.DTO.Dashboard
{
    public class OccupancyByBuildingDTO
    {
        public string Building { get; set; } = string.Empty;
        public int Occupied { get; set; }
        public int Capacity { get; set; }
    }
}