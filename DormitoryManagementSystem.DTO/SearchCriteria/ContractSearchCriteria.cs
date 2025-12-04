using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.SearchCriteria
{
    public class ContractSearchCriteria
    {
        public string? SearchTerm { get; set; } // Tìm chung (Tên, MSSV, Mã HĐ)
        public string? StudentID { get; set; }  // Tìm cụ thể theo SV
        public string? BuildingID { get; set; } // Lọc theo tòa nhà
        public string? Status { get; set; }     // Lọc trạng thái
        public DateOnly? FromDate { get; set; } // Từ ngày
        public DateOnly? ToDate { get; set; }   // Đến ngày
    }
}
