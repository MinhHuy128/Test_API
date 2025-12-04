using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.Contracts
{
    public class ContractDetailDTO
    {
        //hợp đồng
        public string ContractID { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Status { get; set; } = string.Empty; 
        //public DateTime CreatedDate { get; set; }

        //Thông tin sinh viên
        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string Major { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string CCCD { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        // Thông tin phòng ở
        public string RoomID { get; set; } = string.Empty;
        public int RoomNumber { get; set; }
        public string BuildingName { get; set; } = string.Empty;
        //public decimal Price { get; set; }
    }
}
