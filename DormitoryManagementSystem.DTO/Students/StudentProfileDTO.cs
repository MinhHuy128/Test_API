using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.Students
{
    public class StudentProfileDTO
    {
        public string StudentID { get; set; } = string.Empty; 
        public string FullName { get; set; } = string.Empty;  
        public string Major { get; set; } = string.Empty;     
        public string DateOfBirth { get; set; } = string.Empty; 
        public string PhoneNumber { get; set; } = string.Empty; 
        public string Gender { get; set; } = string.Empty;    
        public string Email { get; set; } = string.Empty;     
        public string CCCD { get; set; } = string.Empty;      
        public string Address { get; set; } = string.Empty;   
        //Phong
        public string RoomName { get; set; } = "Chưa có";     
        public string BuildingName { get; set; } = "---";    
        public string ContractStatus { get; set; } = "None";  

        // Thanh tooán
        public decimal AmountToPay { get; set; } = 0;

        public decimal TotalDebt { get; set; } = 0;

        // Trạng thái hiển thị (Đã thanh toán" hoặc "Chưa thanh toán")
        public string PaymentStatusDisplay { get; set; } = "Không có dữ liệu";

        public bool IsDebt { get; set; } = false;
    }
}
