namespace DormitoryManagementSystem.DTO.Rooms
{
    public class RoomReadDTO
    {
        public string RoomID { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public string BuildingID { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public int CurrentOccupancy { get; set; }
        public decimal Price { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool AllowCooking { get; set; }
        public bool AirConditioner { get; set; }

        

        public bool IsFull => CurrentOccupancy >= Capacity;
        public int AvailableSlots => Capacity - CurrentOccupancy;
    }
}