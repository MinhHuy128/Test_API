using System;

namespace DormitoryManagementSystem.DTO.Dashboard
{
    public class AlertDTO
    {
        public string Type { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}