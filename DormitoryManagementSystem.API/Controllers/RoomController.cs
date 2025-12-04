using DormitoryManagementSystem.BUS.Interfaces;
using DormitoryManagementSystem.DTO.Rooms;
using DormitoryManagementSystem.Utils; // AppConstants
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DormitoryManagementSystem.API.Controllers
{
    [Route("api")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomBUS _roomBUS;
        public RoomController(IRoomBUS roomBUS) => _roomBUS = roomBUS;

        // ======================== STUDENT API ========================

        [HttpGet("student/rooms/{id}")]
        [Authorize(Roles = AppConstants.Role.Student)]
        public async Task<IActionResult> GetRoomDetail(string id)
        {
            var room = await _roomBUS.GetRoomDetailByIDAsync(id);
            if (room == null) throw new KeyNotFoundException($"Không tìm thấy phòng: {id}");
            return Ok(room);
        }

        [HttpGet("student/rooms/cards")]
        [Authorize(Roles = AppConstants.Role.Student)]
        public async Task<IActionResult> SearchRoomInCard(
            [FromQuery] string? buildingId, [FromQuery] int? roomNumber,
            [FromQuery] int? capacity, [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice, [FromQuery] bool? allowCooking,
            [FromQuery] bool? airConditioner)
        {
            var result = await _roomBUS.SearchRoomInCardAsync(buildingId, roomNumber, capacity, minPrice, maxPrice, allowCooking, airConditioner);
            return Ok(result);
        }

        [HttpGet("student/rooms/grid")]
        [Authorize(Roles = AppConstants.Role.Student)]
        public async Task<IActionResult> SearchRoomInGrid(
            [FromQuery] string? buildingId, [FromQuery] int? roomNumber,
            [FromQuery] int? capacity, [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var result = await _roomBUS.SearchRoomInGridAsync(buildingId, roomNumber, capacity, minPrice, maxPrice);
            return Ok(result);
        }

        // ======================== COMMON API ========================

        [HttpGet("rooms/capacities")]
        public async Task<IActionResult> GetCapacities() => Ok(await _roomBUS.GetRoomCapacitiesAsync());

        [HttpGet("rooms/price-ranges")]
        public IActionResult GetPriceRanges() => Ok(_roomBUS.GetPriceRanges());

        // ======================== ADMIN API ========================

        [HttpGet("admin/rooms/search")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> SearchRooms([FromQuery] string q)
        {
            var result = await _roomBUS.SearchRoomsAsync(q);
            return Ok(result);
        }

        [HttpPost("admin/rooms")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> CreateRoom([FromBody] RoomCreateDTO dto)
        {
            await _roomBUS.AddRoomAsync(dto);
            return Ok(new { message = "Thêm phòng thành công!" });
        }

        [HttpPut("admin/rooms/{id}")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> UpdateRoom(string id, [FromBody] RoomUpdateDTO dto)
        {
            await _roomBUS.UpdateRoomAsync(id, dto);
            return Ok(new { message = "Cập nhật thông tin phòng thành công!" });
        }

        [HttpDelete("admin/rooms/{id}")]
        [Authorize(Roles = AppConstants.Role.Admin)]
        public async Task<IActionResult> DeleteRoom(string id)
        {
            await _roomBUS.DeleteRoomAsync(id);
            return Ok(new { message = "Xóa phòng thành công" });
        }
    }
}