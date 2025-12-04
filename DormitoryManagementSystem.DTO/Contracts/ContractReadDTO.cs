namespace DormitoryManagementSystem.DTO.Contracts
{
    public class ContractReadDTO
    {
        public string ContractID { get; set; } = string.Empty;
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RoomID { get; set; } = string.Empty;
        public string RoomNumber { get; set; } = string.Empty; 
        public string? StaffUserID { get; set; }
        public DateOnly StartTime { get; set; }
        public DateOnly EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
    }
}