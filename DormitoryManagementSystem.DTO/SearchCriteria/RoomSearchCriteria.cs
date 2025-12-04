using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.SearchCriteria
{
    public class RoomSearchCriteria
    {
        public string? Keyword { get; set; } // Tìm theo tên/mã
        public string? BuildingID { get; set; }
        public int? RoomNumber { get; set; }
        public int? Capacity { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool? AllowCooking { get; set; }
        public bool? AirConditioner { get; set; }
        public string? Status { get; set; } // Active, Inactive...
    }
}
