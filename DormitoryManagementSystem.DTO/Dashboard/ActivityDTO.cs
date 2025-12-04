using System;

namespace DormitoryManagementSystem.DTO.Dashboard
{
    public class ActivityDTO
    {
        public DateTime Time { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}