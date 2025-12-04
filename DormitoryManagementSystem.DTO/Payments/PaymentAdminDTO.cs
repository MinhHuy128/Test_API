using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.Payments
{
    public class PaymentAdminDTO
    {
        public string PaymentID { get; set; } = string.Empty;
        public string ContractID { get; set; } = string.Empty;


        public string StudentID { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RoomName { get; set; } = string.Empty;


        // bill

        public int BillMonth { get; set; }

        public decimal PaymentAmount { get; set; }
        public string PaymentStatus { get; set; } = string.Empty; 
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
    }
}
