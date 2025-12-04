using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DormitoryManagementSystem.DTO.Rooms
{
    public class RoomPriceDTO
    {
        public string DisplayText { get; set; } = string.Empty; 
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}
