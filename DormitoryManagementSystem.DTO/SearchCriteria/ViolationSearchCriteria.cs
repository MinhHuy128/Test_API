using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.SearchCriteria
{
    public class ViolationSearchCriteria
    {
        public string? Keyword { get; set; } // Tên SV, MSSV
        public string? StudentID { get; set; }
        public string? RoomID { get; set; }
        public string? Status { get; set; } // Pending, Resolved...
    }
}
