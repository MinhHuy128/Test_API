using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.SearchCriteria
{
    public class PaymentSearchCriteria
    {
        public string? Keyword { get; set; } // Tên SV, MSSV
        public string? StudentID { get; set; }
        public string? ContractID { get; set; }
        public string? BuildingID { get; set; }
        public string? Status { get; set; } // Paid, Unpaid...
        public int? Month { get; set; }
        public int? Year { get; set; }
    }
}
